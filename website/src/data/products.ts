import type { Product } from "@/domain/product";
import { REPO_URL } from "@/config/site";
import iconCleaner from "../../../src/SirHurt.Cleaner.CLI/Assets/favicon.ico";
import iconCli from "../../../src/Sirstrap.CLI/Assets/favicon.ico";
import iconUi from "../../../src/Sirstrap.UI/Assets/favicon.ico";
import shotUiMain from "../../../images/Sirstrap.png";
import shotUiSettings from "../../../images/Settings.png";

export const products: Product[] = [
  {
    name: "SirHurt.Cleaner.CLI",
    description:
      "A complete cleanup utility that wipes Roblox, SirHurt, and Sirstrap from your filesystem and registry.",
    asset: "SirHurt.Cleaner.CLI.zip",
    variants: ["SirHurt.Cleaner.CLI_fat.zip", "SirHurt.Cleaner.CLI.cab", "SirHurt.Cleaner.CLI_fat.cab"],
    recommended: false,
    icon: iconCleaner,
    source: `${REPO_URL}/src/branch/main/src/SirHurt.Cleaner.CLI`,
    accent: "red",
    title: { kind: "typewriter", head: "SirHurt.Cleaner", tail: ".CLI" },
    core: true,
  },
  {
    name: "Sirstrap.CLI",
    description:
      "An alternative Roblox bootstrapper CLI packed with additional features.",
    asset: "Sirstrap.CLI.zip",
    variants: ["Sirstrap.CLI_fat.zip", "Sirstrap.CLI.cab", "Sirstrap.CLI_fat.cab", "Sirstrap.exe"],
    recommended: false,
    icon: iconCli,
    source: `${REPO_URL}/src/branch/main/src/Sirstrap.CLI`,
    accent: "teal",
    title: { kind: "typewriter", head: "Sirstrap", tail: ".CLI" },
    core: true,
  },
  {
    name: "Sirstrap.UI",
    description:
      "An alternative Roblox bootstrapper UI packed with additional features.",
    asset: "Sirstrap.UI.zip",
    variants: ["Sirstrap.UI_fat.zip", "Sirstrap.UI.cab", "Sirstrap.UI_fat.cab"],
    recommended: true,
    icon: iconUi,
    source: `${REPO_URL}/src/branch/main/src/Sirstrap.UI`,
    accent: "amber",
    title: { kind: "shimmer", head: "Sirstrap", tail: ".UI" },
    core: true,
    screenshots: [
      { src: shotUiMain, alt: "Sirstrap.UI main window" },
      { src: shotUiSettings, alt: "Sirstrap.UI settings window" },
    ],
  },
];
