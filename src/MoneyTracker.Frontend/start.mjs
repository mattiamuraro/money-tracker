import { spawn } from 'node:child_process';
import { resolve } from 'node:path';

const port = process.env.PORT ?? '58100';
const host = process.env.HOST ?? '0.0.0.0';
const ngCliPath = resolve(process.cwd(), 'node_modules', '@angular', 'cli', 'bin', 'ng.js');
const args = [ngCliPath, 'serve', '--host', host, '--port', port];

const child = spawn(process.execPath, args, {
  stdio: 'inherit',
  env: process.env,
});

child.on('exit', (code) => {
  process.exit(code ?? 0);
});

child.on('error', (error) => {
  console.error(error);
  process.exit(1);
});
