import fs from "node:fs";

const sourcePath = String.raw`C:\github\ProjectBS-agent\route_g2_main.js`;
let code = fs.readFileSync(sourcePath, "utf8");
code = code.replace(
  'const repo = String.raw`C:\\Users\\parkv\\.codex\\worktrees\\178d\\ProjectBS-agent`;',
  'const repo = String.raw`C:\\Users\\parkv\\.codex\\worktrees\\178d\\ProjectBS-agent`;\nconst authoritativeProjectRoot = String.raw`C:\\github\\ProjectBS-agent`;'
);
code = code.replace(
  "const identityBytes = readLocal(identityAsset.path);",
  'const identityWorktreePath = path.join(repo, ...identityAsset.path.split("/"));\nconst identityBytes = fs.existsSync(identityWorktreePath) ? fs.readFileSync(identityWorktreePath) : fs.readFileSync(path.join(authoritativeProjectRoot, ...identityAsset.path.split("/")));'
);
await import(`data:text/javascript;base64,${Buffer.from(code, "utf8").toString("base64")}`);
