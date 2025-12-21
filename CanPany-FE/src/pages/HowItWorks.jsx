import Button from '../components/ui/button'
import { useI18n } from '../hooks/useI18n'

export default function HowItWorks() {
  const { t } = useI18n();
    return (
        <div>
            <section className="bg-gradient-to-r from-blue-50 to-orange-50 border-b">
                <div className="container-ld py-16 text-center">
                    <h1 className="text-4xl font-bold">{t("HowItWorks.Title")} <span className="text-brand-700">LanServe</span></h1>
                    <p className="mt-3 text-slate-600">{t("HowItWorks.Subtitle")}</p>
                    <Button className="mt-6">{t("HowItWorks.StartProject")}</Button>
                </div>
            </section>
            <section className="container-ld py-12 grid md:grid-cols-3 gap-5">
                {[{ t: t("HowItWorks.PostProject"), d: [t("HowItWorks.PostProjectDesc1"), t("HowItWorks.PostProjectDesc2"), t("HowItWorks.PostProjectDesc3")] }, { t: t("HowItWorks.ChooseFreelancer"), d: [t("HowItWorks.ChooseFreelancerDesc1"), t("HowItWorks.ChooseFreelancerDesc2"), t("HowItWorks.ChooseFreelancerDesc3")] }, { t: t("HowItWorks.ReceiveProduct"), d: [t("HowItWorks.ReceiveProductDesc1"), t("HowItWorks.ReceiveProductDesc2"), t("HowItWorks.ReceiveProductDesc3")] }].map(b => (
                    <div key={b.t} className="card p-6">
                        <div className="text-3xl">🧩</div>
                        <div className="mt-3 font-semibold">{b.t}</div>
                        <ul className="mt-2 text-sm text-slate-600 list-disc list-inside space-y-1">{b.d.map(x => <li key={x}>{x}</li>)}</ul>
                    </div>
                ))}
            </section>
        </div>
    )
}