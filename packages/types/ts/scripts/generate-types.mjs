import { compileFromFile } from 'json-schema-to-typescript';
import { promises as fs } from 'fs';
import path from 'path';

const root = path.resolve(process.cwd(), '../../../');
const schemasDir = path.join(root, 'packages/contracts/schemas');
const outDir = path.join(process.cwd(), 'src/generated');

async function main() {
  await fs.mkdir(outDir, { recursive: true });
  const files = await fs.readdir(schemasDir);
  const schemaFiles = files.filter((f) => f.endsWith('.json'));
  const barrel = [];

  for (const file of schemaFiles) {
    const inPath = path.join(schemasDir, file);
    const baseName = path.basename(file, '.json');
    const outPath = path.join(outDir, `${baseName}.d.ts`);
    const ts = await compileFromFile(inPath, {
      bannerComment: `/* tslint:disable */\n/* eslint-disable */\n/** Auto-generated from ${file} */`,
      unreachableDefinitions: true,
    });
    await fs.writeFile(outPath, ts, 'utf8');
    barrel.push(`export * from './${baseName}';`);
  }

  await fs.writeFile(path.join(outDir, 'index.d.ts'), barrel.join('\n') + '\n', 'utf8');
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});

