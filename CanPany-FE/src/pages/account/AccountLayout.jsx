import { Outlet, NavLink, useLocation, useParams } from "react-router-dom";
import { useEffect, useState } from "react";
import apiService from "../../lib/api.service";
import { jwtDecode } from "jwt-decode";
import { useI18n } from "../../hooks/useI18n";

export default function AccountLayout() {
  const [profile, setProfile] = useState(null);
  const [rating, setRating] = useState({ avg: "-", count: 0 });
  const [isEditingProfile, setIsEditingProfile] = useState(false);
  const [isOwner, setIsOwner] = useState(false);
  const location = useLocation();
  const search = location.search || "";
  const params = useParams(); // 👈 để nhận userId nếu có

  useEffect(() => {
    const token =
      localStorage.getItem("token") || sessionStorage.getItem("token");
    if (!token) return;

    const decoded = jwtDecode(token);
    const currentUserId =
      decoded[
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
      ];

    const viewedUserId = params.userId || currentUserId;
    if (!viewedUserId) return;

    // 👇 tải profile của user được xem (có thể là mình hoặc người khác)
    apiService
      .get(`/userprofiles/by-user/${viewedUserId}`)
      .then((data) => {
        setProfile((prev) => ({ ...(prev || {}), ...data }));
        setIsOwner(viewedUserId === currentUserId);
      })
      .catch((err) => console.error("Profile error:", err));

    // 👇 Lấy thông tin User để có avatarUrl và fullName
    if (viewedUserId === currentUserId) {
      apiService
        .get("/users/me")
        .then((data) => {
          const fullName = data?.fullName ?? data?.name ?? "";
          const avatarUrl = data?.avatarUrl ?? "";
          setProfile((prev) => ({ ...(prev || {}), fullName, avatarUrl }));
        })
        .catch((err) => console.error("User error:", err));
    } else {
      // 👇 nếu là người khác thì lấy tên qua API /users/:id
      apiService
        .get(`/users/${viewedUserId}`)
        .then((data) => {
          const fullName = data?.fullName ?? data?.name ?? "";
          const avatarUrl = data?.avatarUrl ?? "";
          setProfile((prev) => ({ ...(prev || {}), fullName, avatarUrl }));
        })
        .catch((err) => console.error("User (viewed) error:", err));
    }

    // 👇 lấy đánh giá của user được xem
    apiService
      .get(`/reviews/by-user/${viewedUserId}`)
      .then((reviews) => {
        const reviewList = reviews || [];
        if (reviewList.length > 0) {
          const total = reviewList.reduce(
            (sum, r) => sum + (Number(r.rating) || 0),
            0
          );
          const avg = total / reviewList.length;
          setRating({ avg: avg.toFixed(1), count: reviewList.length });
        } else {
          setRating({ avg: "-", count: 0 });
        }
      })
      .catch((err) => console.error("Review error:", err));
  }, [params.userId]);

  const { t } = useI18n();

  if (!profile) return <p className="p-4">{t("Account.Loading")}</p>;

  const tabs = [
    { to: "profile", label: t("Account.PersonalProfile") },
    { to: "my-projects", label: t("Account.MyJobs") },
    { to: "cvs", label: t("Account.MyCVs") },
    { to: "proposals", label: t("Account.Applications") },
    { to: "messages", label: t("Account.Messages") },
    { to: "companies", label: t("Account.Companies") },
    { to: "settings", label: t("Account.Settings") },
  ];

  return (
    <div className="container-ld py-8">
      <div className="card overflow-hidden">
        <div className="h-28 bg-gradient-to-r from-blue-600 to-orange-500" />

        <div className="p-5 flex items-center justify-between">
          <div className="flex items-center gap-4">
            {/* Avatar */}
            {profile.avatarUrl ? (
              <img
                src={profile.avatarUrl}
                alt="Avatar"
                className="w-16 h-16 rounded-full object-cover border-2 border-white"
              />
            ) : (
              <div className="w-16 h-16 rounded-full bg-slate-300 flex items-center justify-center text-2xl">
                👤
              </div>
            )}
          <div>
            <div className="text-xl font-semibold">
              {profile.fullName && profile.fullName.trim()
                ? profile.fullName
                : t("Account.HiddenUser")}
              <span className="badge badge-success ml-2">{t("Account.Verified")}</span>
            </div>
            <div className="text-sm text-slate-600">
              {profile.title ?? t("Account.NoJobTitle")} •{" "}
              {profile.location ?? t("Account.NoLocation")} • {t("Account.Joined")}{" "}
              {profile.createdAt
                ? new Date(profile.createdAt).toLocaleDateString("vi-VN")
                : "-"}{" "}
              • ⭐ {rating.avg}/5{" "}
              {rating.count > 0 ? `(${rating.count} ${t("Account.Reviews")})` : ""}
              </div>
            </div>
          </div>

          {/* 👇 Chỉ hiển thị nếu là chủ sở hữu */}
          {isOwner && (
            <button
              className="btn btn-outline"
              onClick={() => setIsEditingProfile((prev) => !prev)}
            >
              {isEditingProfile ? `${t("Common.Cancel")} ${t("Common.Edit")}` : `${t("Common.Edit")} ${t("Account.PersonalProfile")}`}
            </button>
          )}
        </div>

        {/* 👇 Tabs chỉ hiện khi là chủ sở hữu */}
        {isOwner && (
          <div className="px-5 border-t border-slate-100">
            <nav className="flex gap-2 overflow-x-auto">
              {tabs.map((t) => (
                <NavLink
                  key={t.to}
                  to={{ pathname: t.to, search }}
                  className={({ isActive }) =>
                    `tab ${isActive ? "tab-active" : ""}`
                  }
                >
                  {t.label}
                </NavLink>
              ))}
            </nav>
          </div>
        )}
      </div>

      <div className="mt-6">
        <Outlet context={{ isEditingProfile, setIsEditingProfile }} />
      </div>
    </div>
  );
}
