import Button from '../components/ui/button'
import { useEffect, useState } from 'react'
import api from '../lib/api'
import { useNavigate, Link } from "react-router-dom";
import BannerCarousel from '../components/BannerCarousel';
import { jwtDecode } from 'jwt-decode';
import { useI18n } from '../hooks/useI18n';

export default function Home() {
  const [categories, setCategories] = useState([]);
  const [freelancers, setFreelancers] = useState([]);
  const [recommendedProjects, setRecommendedProjects] = useState([]);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const navigate = useNavigate();
  const { t } = useI18n();

  useEffect(() => {
    api.get("/categories")
      .then(res => setCategories(res.data ?? []))
      .catch(err => console.error("Load categories failed", err));

    // dùng /users vì BE chưa có /userprofiles
    api.get("/users")
      .then(res => setFreelancers(res.data ?? []))
      .catch(err => console.error("Load freelancers failed", err));

    // Load recommended projects nếu đã đăng nhập
    const token = localStorage.getItem("token") || sessionStorage.getItem("token");
    if (token) {
      try {
        setIsLoggedIn(true);
        api.get("/projects/recommended?limit=6")
          .then(res => {
            const data = res.data || [];
            setRecommendedProjects(data.map(item => ({
              ...item.project,
              similarity: item.similarity
            })));
          })
          .catch(err => {
            console.error("Load recommended projects failed", err);
            // Nếu lỗi 401, user chưa có profile hoặc chưa có skills
          });
      } catch (e) {
        console.error("Token decode error:", e);
      }
    }
  }, []);

  return (
    <div>
      {/* Banners Carousel */}
      <section className="container-ld py-8">
        <BannerCarousel />
      </section>

      {/* Hero */}
      <section className="bg-gradient-to-r from-blue-50 to-orange-50 border-b">
        <div className="container-ld py-16">
          <div className="text-center">
            <h1 className="text-4xl md:text-5xl font-bold leading-tight">
              {t("Home.HeroTitle")} <span className="text-brand-700">{t("Home.Candidate")}</span> & <span className="text-accent">{t("Home.Company")}</span>
            </h1>
            <p className="mt-4 text-slate-600 max-w-2xl mx-auto">
              {t("Home.HeroSubtitle")}
            </p>
            <div className="mt-6 flex gap-3 justify-center">
              <Button>{t("Home.FindFreelancer")}</Button>
              <Button variant="outline" as={Link} to="/post-project">{t("Home.PostProject")}</Button>
            </div>
          </div>
        </div>
      </section>

      {/* Categories */}
      <section className="container-ld py-12">
        <h2 className="text-2xl font-semibold">{t("Home.ServiceCategories")}</h2>
        <div className="mt-6 grid md:grid-cols-3 lg:grid-cols-4 gap-5">
          {categories.map((c, i) => (
            <div key={c.id || c._id || `cat-${i}`} className="card p-5">
              <div className="text-xl">📦</div>
              <div className="mt-3 font-medium">{c.name}</div>
            </div>
          ))}
        </div>
      </section>

      {/* Recommended Projects (chỉ hiển thị khi đã đăng nhập) */}
      {isLoggedIn && recommendedProjects.length > 0 && (
        <section className="bg-gradient-to-br from-blue-50 to-purple-50 border-y">
          <div className="container-ld py-12">
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-semibold">{t("Home.SuitableProjects")}</h2>
              <Link to="/account/projects" className="text-brand-700 hover:underline text-sm">
                {t("Home.ViewAll")}
              </Link>
            </div>
            <div className="mt-6 grid md:grid-cols-2 lg:grid-cols-3 gap-5">
              {recommendedProjects.map((project) => (
                <div key={project.id} className="card p-5 hover:shadow-lg transition-shadow">
                  <div className="flex items-start justify-between mb-3">
                    <h3 className="font-semibold text-lg flex-1">{project.title}</h3>
                    {project.similarity !== undefined && (
                      <span className="ml-2 px-2 py-1 bg-green-100 text-green-700 rounded-full text-xs font-medium">
                        {project.similarity}{t("Home.MatchPercentage")}
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-slate-600 line-clamp-2 mb-3">
                    {project.description}
                  </p>
                  <div className="flex items-center justify-between">
                    <div className="text-brand-700 font-semibold">
                      {project.budgetAmount?.toLocaleString("vi-VN") ?? "—"} đ
                    </div>
                    <Button 
                      variant="outline" 
                      size="sm"
                      as={Link}
                      to={`/account/projects?view=${project.id}`}
                    >
                      {t("Home.ViewDetails")}
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>
      )}

      {/* Freelancers */}
      <section className="bg-white border-y">
        <div className="container-ld py-12">
          <h2 className="text-2xl font-semibold">{t("Home.FeaturedFreelancers")}</h2>
          <div className="mt-6 grid md:grid-cols-3 gap-5">
            {freelancers.slice(0, 3).map((f, i) => (
              <div key={f.id || `user-${i}`} className="card p-5">
                <div className="flex items-center gap-3">
                  <div className="h-12 w-12 rounded-full bg-slate-200" />
                  <div>
                    <div className="font-medium">{f.fullName}</div>
                    <div className="text-sm text-slate-500">{f.email}</div>
                  </div>
                </div>
                <Button className="mt-4 w-full" variant="outline">{t("Home.ViewProfile")}</Button>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="bg-gradient-to-r from-blue-500 to-orange-500 text-white">
        <div className="container-ld py-14 text-center">
          <h2 className="text-3xl font-semibold">{t("Home.ReadyToStart")}</h2>
          <div className="mt-6 flex gap-3 justify-center">
            <Button className="bg-white text-slate-900">{t("Home.SignUpFree")}</Button>
            <Button variant="outline" className="border-white text-black">{t("Home.LearnMore")}</Button>
          </div>
        </div>
      </section>
    </div>
  );
}
