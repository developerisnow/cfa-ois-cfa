'use client';

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '@/lib/api-client';
import { legacyApiClient } from '@/lib/api-client';
import { AppShell, PageHeader, DataTable, EmptyState, Skeleton } from '../../../../shared-ui/src';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { useState, useEffect, useRef } from 'react';
import { ColumnDef } from '@tanstack/react-table';
import { CheckCircle, XCircle, Upload, FileText, Eye } from 'lucide-react';
import { toast } from 'sonner';
import Link from 'next/link';

interface KycApplication {
  investorId: string;
  investorName?: string;
  investorEmail?: string;
  kycStatus: 'pending' | 'approved' | 'rejected';
  qualificationStatus: 'none' | 'qualified' | 'unqualified';
  submittedAt: string;
  documentsCount?: number;
}

export default function KycPage() {
  const { data: session, status } = useSession();
  const router = useRouter();
  const queryClient = useQueryClient();
  const [selectedInvestor, setSelectedInvestor] = useState<string | null>(null);
  const [decisionComment, setDecisionComment] = useState('');
  const [uploadDocumentType, setUploadDocumentType] = useState<string>('passport');
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (status === 'unauthenticated') {
      router.push('/auth/signin');
    }
  }, [status, router]);

  // Mock data - в реальности будет GET /v1/kyc/applications
  const { data: applications, isLoading: applicationsLoading } = useQuery({
    queryKey: ['kyc-applications'],
    queryFn: async () => {
      // TODO: Replace with actual endpoint
      // const response = await apiClient.get('/v1/kyc/applications');
      // return response.data;
      
      // Mock data
      return {
        items: [
          {
            investorId: '123e4567-e89b-12d3-a456-426614174000',
            investorName: 'Иван Иванов',
            investorEmail: 'ivan@example.com',
            kycStatus: 'pending' as const,
            qualificationStatus: 'none' as const,
            submittedAt: new Date().toISOString(),
            documentsCount: 2,
          },
        ],
        total: 1,
      };
    },
    enabled: status === 'authenticated',
  });

  const { data: investorStatus, isLoading: statusLoading } = useQuery({
    queryKey: ['investor-status', selectedInvestor],
    queryFn: async () => {
      if (!selectedInvestor) return null;
      try {
        const response = await legacyApiClient.getInvestorStatus({ id: selectedInvestor });
        return response.data;
      } catch (error) {
        // Fallback: return mock status if endpoint not available
        return {
          kyc: 'pending',
          qualificationTier: 'none',
          qualificationLimit: null,
          qualificationUsed: 0,
        };
      }
    },
    enabled: !!selectedInvestor && status === 'authenticated',
  });

  const { data: documents, isLoading: documentsLoading } = useQuery({
    queryKey: ['kyc-documents', selectedInvestor],
    queryFn: async () => {
      if (!selectedInvestor) return null;
      const response = await apiClient.getKycDocuments(selectedInvestor);
      return response.data;
    },
    enabled: !!selectedInvestor && status === 'authenticated',
  });

  const decisionMutation = useMutation({
    mutationFn: async ({ investorId, status, comment }: { investorId: string; status: 'approved' | 'rejected'; comment: string }) => {
      const response = await apiClient.makeKycDecision(investorId, {
        status,
        comment,
      });
      return response.data;
    },
    onSuccess: (data, variables) => {
      toast.success(`KYC ${variables.status === 'approved' ? 'approved' : 'rejected'} successfully`);
      queryClient.invalidateQueries({ queryKey: ['kyc-applications'] });
      queryClient.invalidateQueries({ queryKey: ['investor-status', variables.investorId] });
      setSelectedInvestor(null);
      setDecisionComment('');
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.detail || 'Failed to make KYC decision');
    },
  });

  const uploadMutation = useMutation({
    mutationFn: async ({ investorId, files, documentType, comment }: { investorId: string; files: File[]; documentType: string; comment?: string }) => {
      const formData = new FormData();
      files.forEach((file) => {
        formData.append('files', file);
      });
      formData.append('documentType', documentType);
      if (comment) {
        formData.append('comment', comment);
      }

      const response = await apiClient.uploadKycDocuments(investorId, formData);
      return response.data;
    },
    onSuccess: () => {
      toast.success('Documents uploaded successfully');
      queryClient.invalidateQueries({ queryKey: ['kyc-documents', selectedInvestor] });
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.detail || 'Failed to upload documents');
    },
  });

  const handleApprove = () => {
    if (!selectedInvestor || !decisionComment.trim()) {
      toast.error('Please enter a comment');
      return;
    }
    decisionMutation.mutate({
      investorId: selectedInvestor,
      status: 'approved',
      comment: decisionComment,
    });
  };

  const handleReject = () => {
    if (!selectedInvestor || !decisionComment.trim()) {
      toast.error('Please enter a comment');
      return;
    }
    decisionMutation.mutate({
      investorId: selectedInvestor,
      status: 'rejected',
      comment: decisionComment,
    });
  };

  const handleUpload = () => {
    if (!selectedInvestor || !fileInputRef.current?.files || fileInputRef.current.files.length === 0) {
      toast.error('Please select files to upload');
      return;
    }
    const files = Array.from(fileInputRef.current.files);
    uploadMutation.mutate({
      investorId: selectedInvestor,
      files,
      documentType: uploadDocumentType,
    });
  };

  if (status === 'loading') {
    return <div className="p-8">Loading...</div>;
  }

  if (!session) {
    return null;
  }

  const columns: ColumnDef<KycApplication>[] = [
    {
      accessorKey: 'investorName',
      header: 'Investor',
      cell: ({ row }) => (
        <div>
          <div className="font-medium text-text-primary">{row.original.investorName || 'Unknown'}</div>
          <div className="text-sm text-text-secondary">{row.original.investorEmail}</div>
        </div>
      ),
    },
    {
      accessorKey: 'kycStatus',
      header: 'KYC Status',
      cell: ({ row }) => {
        const status = row.original.kycStatus;
        const colors = {
          approved: 'bg-success-100 text-success-700 dark:bg-success-900 dark:text-success-300',
          rejected: 'bg-danger-100 text-danger-700 dark:bg-danger-900 dark:text-danger-300',
          pending: 'bg-warning-100 text-warning-700 dark:bg-warning-900 dark:text-warning-300',
        };
        return (
          <span className={`px-2 py-1 text-xs font-medium rounded ${colors[status]}`}>
            {status}
          </span>
        );
      },
    },
    {
      accessorKey: 'qualificationStatus',
      header: 'Qualification',
      cell: ({ row }) => {
        const status = row.original.qualificationStatus;
        const colors: Record<string, string> = {
          qualified: 'bg-success-100 text-success-700 dark:bg-success-900 dark:text-success-300',
          unqualified: 'bg-danger-100 text-danger-700 dark:bg-danger-900 dark:text-danger-300',
          none: 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300',
        };
        return (
          <span className={`px-2 py-1 text-xs font-medium rounded ${colors[status] || ''}`}>
            {status}
          </span>
        );
      },
    },
    {
      accessorKey: 'submittedAt',
      header: 'Submitted',
      cell: ({ row }) => (
        <span className="text-sm text-text-secondary">
          {new Date(row.original.submittedAt).toLocaleDateString()}
        </span>
      ),
    },
    {
      id: 'actions',
      header: 'Actions',
      cell: ({ row }) => (
        <button
          onClick={() => setSelectedInvestor(row.original.investorId)}
          className="flex items-center gap-1 text-primary-600 hover:text-primary-700"
        >
          <Eye className="h-4 w-4" />
          View
        </button>
      ),
    },
  ];

  return (
    <AppShell
      user={session.user}
      sidebar={{
        items: [
          { label: 'Dashboard', href: '/' },
          { label: 'KYC', href: '/kyc' },
          { label: 'Qualification', href: '/qualification' },
          { label: 'Audit', href: '/audit' },
          { label: 'Payouts', href: '/payouts' },
        ],
      }}
    >
      <PageHeader
        title="KYC Management"
        description="Manage KYC applications and decisions"
      />

      {!selectedInvestor && (
        <>
          {applicationsLoading && (
            <div className="space-y-4">
              <Skeleton className="h-64 w-full" variant="rectangular" />
            </div>
          )}

          {!applicationsLoading && applications && (
            <DataTable
              columns={columns}
              data={(applications.items || []) as KycApplication[]}
              searchable
              searchPlaceholder="Search investors..."
              pageSize={20}
            />
          )}

          {!applicationsLoading && (!applications || applications.items?.length === 0) && (
            <EmptyState
              title="No KYC applications found"
              description="KYC applications will appear here"
            />
          )}
        </>
      )}

      {selectedInvestor && (
        <div className="space-y-6">
          <div className="flex justify-between items-center">
            <h2 className="text-xl font-semibold text-text-primary">
              Investor: {applications?.items?.find((a: KycApplication) => a.investorId === selectedInvestor)?.investorName || selectedInvestor}
            </h2>
            <button
              onClick={() => {
                setSelectedInvestor(null);
                setDecisionComment('');
              }}
              className="px-4 py-2 border border-border rounded-md bg-surface text-text-primary hover:bg-surface-hover"
            >
              Back to List
            </button>
          </div>

          {/* Status */}
          {statusLoading && <Skeleton className="h-32 w-full" variant="rectangular" />}
          {!statusLoading && investorStatus && (
            <div className="bg-surface border border-border rounded-lg p-6">
              <h3 className="text-lg font-semibold text-text-primary mb-4">Compliance Status</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-text-secondary">KYC Status</p>
                  <p className="text-lg font-medium text-text-primary">{investorStatus.kyc}</p>
                </div>
                <div>
                  <p className="text-sm text-text-secondary">Qualification Tier</p>
                  <p className="text-lg font-medium text-text-primary">{investorStatus.qualificationTier || 'N/A'}</p>
                </div>
              </div>
            </div>
          )}

          {/* Documents */}
          <div className="bg-surface border border-border rounded-lg p-6">
            <h3 className="text-lg font-semibold text-text-primary mb-4">Documents</h3>
            {documentsLoading && <Skeleton className="h-32 w-full" variant="rectangular" />}
            {!documentsLoading && documents && documents.items && documents.items.length > 0 && (
              <div className="space-y-2 mb-4">
                {documents.items.map((doc: any) => (
                  <div key={doc.id} className="flex items-center justify-between p-3 bg-background rounded-md">
                    <div className="flex items-center gap-2">
                      <FileText className="h-4 w-4 text-text-tertiary" />
                      <span className="text-sm text-text-primary">{doc.fileName}</span>
                      <span className="text-xs text-text-tertiary">({doc.documentType})</span>
                    </div>
                    <a
                      href={doc.storageUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-sm text-primary-600 hover:text-primary-700"
                    >
                      View
                    </a>
                  </div>
                ))}
              </div>
            )}

            {/* Upload Documents */}
            <div className="border-t border-border pt-4 mt-4">
              <h4 className="text-md font-semibold text-text-primary mb-3">Upload Documents</h4>
              <div className="space-y-3">
                <div>
                  <label htmlFor="documentType" className="block text-sm font-medium text-text-primary mb-1">
                    Document Type
                  </label>
                  <select
                    id="documentType"
                    value={uploadDocumentType}
                    onChange={(e) => setUploadDocumentType(e.target.value)}
                    className="w-full px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
                  >
                    <option value="passport">Passport</option>
                    <option value="inn">INN</option>
                    <option value="snils">SNILS</option>
                    <option value="address_proof">Address Proof</option>
                    <option value="income_proof">Income Proof</option>
                    <option value="other">Other</option>
                  </select>
                </div>
                <div>
                  <label htmlFor="files" className="block text-sm font-medium text-text-primary mb-1">
                    Files
                  </label>
                  <input
                    ref={fileInputRef}
                    id="files"
                    type="file"
                    multiple
                    className="w-full px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
                  />
                </div>
                <button
                  onClick={handleUpload}
                  disabled={uploadMutation.isPending}
                  className="flex items-center gap-2 px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-primary-500 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <Upload className="h-4 w-4" />
                  {uploadMutation.isPending ? 'Uploading...' : 'Upload Documents'}
                </button>
              </div>
            </div>
          </div>

          {/* Decision */}
          {investorStatus?.kyc === 'pending' && (
            <div className="bg-surface border border-border rounded-lg p-6">
              <h3 className="text-lg font-semibold text-text-primary mb-4">Make Decision</h3>
              <div className="space-y-4">
                <div>
                  <label htmlFor="comment" className="block text-sm font-medium text-text-primary mb-1">
                    Comment *
                  </label>
                  <textarea
                    id="comment"
                    value={decisionComment}
                    onChange={(e) => setDecisionComment(e.target.value)}
                    rows={4}
                    className="w-full px-4 py-2 border border-border rounded-md bg-surface text-text-primary focus:outline-none focus:ring-2 focus:ring-primary-500"
                    placeholder="Enter decision comment..."
                  />
                </div>
                <div className="flex gap-3">
                  <button
                    onClick={handleApprove}
                    disabled={decisionMutation.isPending || !decisionComment.trim()}
                    className="flex items-center gap-2 px-4 py-2 bg-success-600 text-white rounded-md hover:bg-success-700 focus:outline-none focus:ring-2 focus:ring-success-500 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <CheckCircle className="h-4 w-4" />
                    Approve
                  </button>
                  <button
                    onClick={handleReject}
                    disabled={decisionMutation.isPending || !decisionComment.trim()}
                    className="flex items-center gap-2 px-4 py-2 bg-danger-600 text-white rounded-md hover:bg-danger-700 focus:outline-none focus:ring-2 focus:ring-danger-500 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <XCircle className="h-4 w-4" />
                    Reject
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      )}
    </AppShell>
  );
}
