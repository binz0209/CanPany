import { useEffect, useState } from "react";
import { useOutletContext, useParams } from "react-router-dom";
import apiService from "../../lib/api.service";
import { jwtDecode } from "jwt-decode";
import { useI18n } from "../../hooks/useI18n";

export default function Profile() {
  const { t } = useI18n();
  const [profile, setProfile] = useState(null);
  const [skills, setSkills] = useState([]);
  const [allSkills, setAllSkills] = useState([]);
  const [selectedSkill, setSelectedSkill] = useState("");
  const [isOwner, setIsOwner] = useState(false);
  const [user, setUser] = useState(null);
  const [avatarUrl, setAvatarUrl] = useState("");

  const outletContext = useOutletContext?.() || {};
  const { isEditingProfile, setIsEditingProfile } = outletContext;

  const { userId: viewedUserId } = useParams(); // 👈 nếu có userId => đang xem người khác

  useEffect(() => {
    const token =
      localStorage.getItem("token") || sessionStorage.getItem("token");
    let currentUserId = null;

    if (token) {
      const decoded = jwtDecode(token);
      currentUserId =
        decoded.sub ||
        decoded[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        ] ||
        decoded.userId;
    }

    const targetUserId = viewedUserId || currentUserId;

    if (!targetUserId) return;

    // Lấy thông tin User để có avatarUrl
    apiService
      .get(`/users/${targetUserId}`)
      .then((data) => {
        setUser(data);
        setAvatarUrl(data?.avatarUrl || "");
      })
      .catch((err) => console.error("Get user error:", err));

    apiService
      .get(`/userprofiles/by-user/${targetUserId}`)
      .then(async (data) => {
        // Nếu hồ sơ bị ẩn công khai và người xem KHÔNG phải chủ sở hữu → hiển thị thông báo ẩn
        if (data && data.hidden) {
          setProfile({ hidden: true, message: data.message });
          setIsOwner(false);
          return;
        }

        setProfile(data);
        setIsOwner(!viewedUserId && data.userId === currentUserId);

        if (data.skillIds?.length > 0) {
          const skillsData = await apiService.post("/skills/resolve", data.skillIds);
          setSkills(skillsData);
        }
      })
      .catch((err) => {
        console.error("Get profile error:", err?.message || err?.response?.data || err);
      });
  }, [viewedUserId]);

  useEffect(() => {
    if (isEditingProfile && isOwner) {
      api
        .get("/api/skills")
        .then((res) => setAllSkills(res.data))
        .catch((err) => console.error("Load skills error:", err));
    }
  }, [isEditingProfile, isOwner]);

    if (!profile) return <p className="p-4">Đang tải hồ sơ...</p>;
    if (profile.hidden) {
      return (
        <div className="card p-6 text-center text-sm text-gray-600">
          {profile.message || t("Profile.HiddenProfile")}
        </div>
      );
    }

  const handleChange = (e) => {
    const { name, value } = e.target;
    setProfile((prev) => ({ ...prev, [name]: value }));
  };

  const handleArrayChange = (field, value, index) => {
    const updated = [...(profile[field] || [])];
    updated[index] = value;
    setProfile((prev) => ({ ...prev, [field]: updated }));
  };

  const handleAddItem = (field) => {
    setProfile((prev) => ({
      ...prev,
      [field]: [...(prev[field] || []), ""],
    }));
  };

  const handleRemoveItem = (field, index) => {
    const updated = [...(profile[field] || [])];
    updated.splice(index, 1);
    setProfile((prev) => ({ ...prev, [field]: updated }));
  };

  const handleSave = async () => {
    try {
      await apiService.put(`/userprofiles/${profile.id}`, profile);
      // Cập nhật avatar nếu có
      if (avatarUrl && user) {
        await apiService.put(`/users/${user.id}`, { ...user, avatarUrl });
      }
      alert(t("Profile.UpdateSuccess"));
      setIsEditingProfile(false);
      // Reload để hiển thị avatar mới
      window.location.reload();
    } catch (err) {
      console.error("Update error:", err);
      alert(t("Profile.UpdateFailed"));
    }
  };

  const handleAddSkill = async () => {
    if (!selectedSkill || profile.skillIds.includes(selectedSkill)) return; // 👈 tránh trùng skill

    try {
      const updated = {
        ...profile,
        skillIds: [...(profile.skillIds || []), selectedSkill],
      };

      await apiService.put(`/userprofiles/${profile.id}`, updated);

      const skillsData = await apiService.post("/skills/resolve", updated.skillIds);
      setSkills(skillsData);
      setProfile(updated);
      setSelectedSkill("");
    } catch (err) {
      console.error("Add skill error:", err);
      alert(t("Profile.AddSkillFailed"));
    }
  };

  const handleRemoveSkill = async (skillId) => {
    try {
      const updated = {
        ...profile,
        skillIds: profile.skillIds.filter((id) => id !== skillId),
      };

      await apiService.put(`/userprofiles/${profile.id}`, updated);

      const skillsData = await apiService.post("/skills/resolve", updated.skillIds);
      setSkills(skillsData);
      setProfile(updated);
    } catch (err) {
      console.error("Remove skill error:", err);
      alert(t("Profile.RemoveSkillFailed"));
    }
  };

  const isEditing = isOwner && isEditingProfile; // chỉ cho phép edit nếu là chủ

  return (
    <div className="grid lg:grid-cols-3 gap-6">
      {/* Thông tin cá nhân */}
      <div className="space-y-6">
        <div className="card p-5">
          <div className="font-semibold">Thông tin cá nhân</div>

          <div className="mt-3 text-sm grid gap-2">
            {isEditing ? (
              <>
                <input
                  type="text"
                  name="title"
                  value={profile.title || ""}
                  onChange={handleChange}
                  className="input"
                  placeholder={t("Profile.JobTitle")}
                />
                <input
                  type="text"
                  name="location"
                  value={profile.location || ""}
                  onChange={handleChange}
                  className="input"
                  placeholder={t("Profile.Location")}
                />
                <input
                  type="number"
                  name="hourlyRate"
                  value={profile.hourlyRate || ""}
                  onChange={handleChange}
                  className="input"
                  placeholder={t("Profile.HourlyRate")}
                />
              </>
            ) : (
              <>
                <div>
                  {t("Profile.JobTitle")}: <b>{profile.title}</b>
                </div>
                <div>{t("Profile.Location")}: {profile.location ?? t("Profile.NotUpdated")}</div>
                <div>Rate: {profile.hourlyRate ?? "-"} VND/h</div>
              </>
            )}
          </div>
        </div>

        {/* Ngôn ngữ */}
        <div className="card p-5">
          <div className="font-semibold mb-3">{t("Profile.Languages")}</div>
          {isEditing ? (
            <div className="flex flex-col gap-2">
              {profile.languages?.map((lang, i) => (
                <div key={i} className="flex items-center gap-2">
                  <input
                    type="text"
                    value={lang}
                    onChange={(e) =>
                      handleArrayChange("languages", e.target.value, i)
                    }
                    className="input flex-1"
                    placeholder={t("Profile.Languages")}
                  />
                  <button
                    className="btn btn-xs btn-error"
                    onClick={() => handleRemoveItem("languages", i)}
                  >
                    ❌
                  </button>
                </div>
              ))}
              <button
                className="btn btn-xs btn-outline"
                onClick={() => handleAddItem("languages")}
              >
                + {t("Profile.AddLanguage")}
              </button>
            </div>
          ) : (
            <div className="flex gap-2 flex-wrap">
              {profile.languages?.map((lang, i) => (
                <span key={i} className="badge">
                  {lang}
                </span>
              ))}
            </div>
          )}
        </div>

        {/* Chứng chỉ */}
        <div className="card p-5">
          <div className="font-semibold mb-3">{t("Profile.Certificates")}</div>
          {isEditing ? (
            <div className="flex flex-col gap-2">
              {profile.certifications?.map((c, i) => (
                <div key={i} className="flex items-center gap-2">
                  <input
                    type="text"
                    value={c}
                    onChange={(e) =>
                      handleArrayChange("certifications", e.target.value, i)
                    }
                    className="input flex-1"
                    placeholder={t("Profile.Certificates")}
                  />
                  <button
                    className="btn btn-xs btn-error"
                    onClick={() => handleRemoveItem("certifications", i)}
                  >
                    ❌
                  </button>
                </div>
              ))}
              <button
                className="btn btn-xs btn-outline"
                onClick={() => handleAddItem("certifications")}
              >
                + {t("Profile.AddCertificate")}
              </button>
            </div>
          ) : (
            <ul className="text-sm space-y-2">
              {profile.certifications?.map((c, i) => (
                <li key={i}>{c}</li>
              ))}
            </ul>
          )}
        </div>
      </div>

      {/* Giới thiệu & kỹ năng */}
      <div className="lg:col-span-2 space-y-6">
        <div className="card p-5">
          <div className="font-semibold mb-3">{t("Profile.Bio")}</div>
          {isEditing ? (
            <textarea
              name="bio"
              value={profile.bio || ""}
              onChange={handleChange}
              className="textarea w-full"
              placeholder={t("Profile.BioPlaceholder")}
            />
          ) : (
            <p className="text-sm text-slate-700">{profile.bio}</p>
          )}
        </div>

        <div className="card p-5">
          <div className="flex items-center justify-between">
            <div className="font-semibold">{t("Projects.SkillsLabel")}</div>
            {isEditing && (
              <div className="flex gap-2">
                <select
                  className="select"
                  value={selectedSkill}
                  onChange={(e) => setSelectedSkill(e.target.value)}
                >
                  <option value="">-- Chọn kỹ năng --</option>
                  {allSkills.map((skill) => (
                    <option key={skill.id} value={skill.id}>
                      {skill.name}
                    </option>
                  ))}
                </select>
                <button className="btn btn-outline" onClick={handleAddSkill}>
                  + Thêm
                </button>
              </div>
            )}
          </div>

          <div className="mt-4 flex flex-wrap gap-2">
            {skills.map((s) => (
              <span key={s.id} className="badge flex items-center gap-1">
                {s.name}
                {isEditing && (
                  <button
                    className="ml-1 text-red-500"
                    onClick={() => handleRemoveSkill(s.id)}
                  >
                    ❌
                  </button>
                )}
              </span>
            ))}
          </div>
        </div>
      </div>

      {/* Nút lưu */}
      {isEditing && (
        <div className="lg:col-span-3 text-right">
          <button className="btn btn-primary" onClick={handleSave}>
            {t("Projects.SaveChanges")}
          </button>
        </div>
      )}
    </div>
  );
}
