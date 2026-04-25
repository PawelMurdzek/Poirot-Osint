import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const req = JSON.parse(process.env.POIROT_REQUEST || "{}");
let lastSession = null;

const c = new HubConnectionBuilder()
  .withUrl("http://127.0.0.1:57063/osinthub")
  .configureLogging(LogLevel.Warning)
  .build();

c.on("ReceiveNode", n => {
  const v = (n.value || "").toString();
  console.log("[node]", n.label, "->", v.length > 120 ? v.slice(0, 117) + "..." : v);
});
c.on("ReceiveCandidates", a => console.log(`[candidates] ${a.length} total`));
c.on("ReceiveProfile", p => console.log(`[profile] platforms=${(p && p.platforms ? p.platforms.length : 0)}`));
c.on("ReceiveSessionMemory", r => {
  lastSession = r;
  console.log("\n[session] folder=" + r.folderPath);
  console.log("[session] json=" + r.jsonPath);
  console.log("[session] md=" + r.markdownPath);
  console.log("[session] aiConfigured=" + r.claudeApiConfigured);
});
c.on("ReceivePersonalityProfile", p => console.log("[personality]", (p && (p.candidateUsername || p.username)) || "?"));
c.on("SearchStarted", m => console.log("[started]", m));
c.on("SearchError", e => { console.error("[err]", e); process.exit(1); });
c.on("SearchCancelled", m => { console.error("[cancelled]", m); process.exit(2); });
c.on("SearchCompleted", () => {
  console.log("[done]");
  if (lastSession && lastSession.claudeCommand) {
    console.log("\n=========================================================");
    console.log("PASTE THIS INTO ANOTHER TERMINAL FOR LOCAL CLAUDE RANKING:");
    console.log("=========================================================");
    console.log(lastSession.claudeCommand);
    console.log("=========================================================\n");
  }
  c.stop().then(() => process.exit(0));
});

await c.start();
console.log("[connected] invoking StartSearch with", JSON.stringify(req));
await c.invoke("StartSearch", req);
