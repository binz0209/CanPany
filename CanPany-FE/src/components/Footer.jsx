import { useI18n } from "../hooks/useI18n";

export default function Footer() {
  const { t } = useI18n();
    return (
        <footer className="mt-16 bg-slate-900 text-slate-200">
            <div className="container-ld grid md:grid-cols-4 gap-8 py-12">
                <div>
                    <div className="text-lg font-semibold">LanServe</div>
                    <p className="mt-3 text-sm text-slate-400">{t("Footer.Description")}</p>
                    <div className="mt-4 flex gap-3 text-xl"><span>🐦</span><span>💼</span><span>🔗</span></div>
                </div>
                <div>
                    <div className="font-semibold">{t("Footer.QuickLinks")}</div>
                    <ul className="mt-3 space-y-2 text-sm text-slate-300">
                        <li>{t("Navbar.Home")}</li><li>{t("Footer.FindFreelancer")}</li><li>{t("Navbar.PostProject")}</li><li>{t("Navbar.HowItWorks")}</li><li>{t("Footer.AboutUs")}</li>
                    </ul>
                </div>
                <div>
                    <div className="font-semibold">{t("Footer.Support")}</div>
                    <ul className="mt-3 space-y-2 text-sm text-slate-300">
                        <li>{t("Footer.HelpCenter")}</li><li>{t("Footer.Contact")}</li><li>{t("Footer.Terms")}</li><li>{t("Footer.Privacy")}</li><li>{t("Footer.FAQ")}</li>
                    </ul>
                </div>
                <div className="md:text-right text-sm text-slate-400 flex items-end">© 2024 LanServe.</div>
            </div>
        </footer>
    )
}