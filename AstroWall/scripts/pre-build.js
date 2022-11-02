const fs = require("fs");
const plist = require("plist");
const { promisify } = require("util");
const exec = promisify(require("child_process").exec);

(async () => {
  // read XML file
  fs.readFile("info.plist", "utf-8", async (err, data) => {
    if (err) {
      throw err;
    }

    var plistObj = plist.parse(data);

    // Get versions from git tag
    var version = (await exec("git describe")).stdout.slice(1).slice(0, -1);
    var shortVersion = version.split("-")[0];

    // update info.plist
    plistObj["CFBundleVersion"] = version;
    plistObj["CFBundleShortVersionString"] = shortVersion;
    console.log(JSON.stringify(plistObj, null, 2));
    const xml = plist.build(plistObj);
    console.log(xml);
    fs.writeFileSync("info.plist", xml);
  });
})();
