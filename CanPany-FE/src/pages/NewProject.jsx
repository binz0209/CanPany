import { useState, useEffect } from "react";
import Input from "../components/ui/input";
import Textarea from "../components/ui/textarea";
import Select from "../components/ui/select";
import Button from "../components/ui/button";
import ImageUpload from "../components/ImageUpload";
import api from "../lib/axios";
import { jwtDecode } from "jwt-decode";
import { useNavigate } from "react-router-dom";
import { useI18n } from "../hooks/useI18n";

export default function NewProject() {
  const navigate = useNavigate();
  const { t } = useI18n();

  // form state
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [budgetType, setBudgetType] = useState("Fixed");
  const [budgetAmount, setBudgetAmount] = useState(5000000);
  const [deadline, setDeadline] = useState("");
  const [skills, setSkills] = useState([]);
  const [images, setImages] = useState([]);

  // options
  const [categories, setCategories] = useState([]);
  const [skillOptions, setSkillOptions] = useState([]);

  // auth
  const [currentUserId, setCurrentUserId] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [err, setErr] = useState("");

  // ---- helpers ----
  const normId = (x) =>
    (x ?? "")
      .toString()
      .trim()
      .replace(/^ObjectId\(["']?(.+?)["']?\)$/i, "$1");

  useEffect(() => {
    const token =
      localStorage.getItem("token") || sessionStorage.getItem("token");
    if (!token) return;
    try {
      const dec = jwtDecode(token);
      const id =
        dec.sub ||
        dec[
          "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        ] ||
        dec.userId ||
        dec.uid ||
        "";
      setCurrentUserId(normId(id));
    } catch (e) {
      console.error("Decode token error:", e);
    }
  }, []);

  // load categories + skills từ backend
  useEffect(() => {
    (async () => {
      try {
        const [catRes, skillRes] = await Promise.all([
          api.get("/categories"),
          api.get("/skills"),
        ]);
        setCategories(catRes.data ?? []);
        setSkillOptions(skillRes.data ?? []);
      } catch (e) {
        console.error("Load categories/skills failed", e);
      }
    })();
  }, []);

  const toggleSkill = (id) => {
    setSkills((prev) =>
      prev.includes(id) ? prev.filter((s) => s !== id) : [...prev, id]
    );
  };

  const validate = () => {
    if (!currentUserId) return t("Common.NotLoggedIn");
    if (!title.trim()) return t("Projects.PleaseEnterTitle");
    if (!description.trim()) return t("Projects.PleaseEnterDescription");
    if (!categoryId) return t("Projects.PleaseSelectCategory");
    if (!budgetAmount || Number(budgetAmount) <= 0)
      return t("Projects.InvalidBudget");
    if (!deadline) return t("NewProject.PleaseSelectDeadline");
    return "";
  };

  // NEW: chỉ kiểm tra số dư, không trừ tiền ở bước tạo project
  async function getWalletBalance(userId) {
    if (!userId) return 0;
    try {
      // BE: GET /api/wallets/{userId} -> { balance }
      const res = await api.get(`/wallets/${userId}`);
      return Number(res.data?.balance || 0);
    } catch {
      return 0;
    }
  }

  const handleSubmit = async () => {
    const v = validate();
    if (v) {
      setErr(v);
      return;
    }
    setErr("");
    try {
      setSubmitting(true);

      const deadlineIso = deadline
        ? new Date(`${deadline}T00:00:00`).toISOString()
        : null;

      // Map form -> Job entity
      const salary = budgetAmount ? Number(budgetAmount) : 0;
      const payload = {
        // TẠM THỜI: dùng userId như companyId (sau này có company thật sẽ map lại)
        companyId: currentUserId,
        title,
        description,
        requiredSkills: skills, // danh sách skillId
        jobType: budgetType === "Hourly" ? "Contract" : "FullTime",
        location: "", // có thể bổ sung trường location riêng sau
        salaryRange:
          salary > 0
            ? { min: salary, max: salary, currency: "VND" }
            : null,
        experienceLevel: "Mid",
        status: "Open",
        premiumBoost: false,
        images,
        // deadline vẫn lưu tạm trong description hoặc sẽ thêm field ở backend nếu cần
      };

      // ① KIỂM TRA SỐ DƯ (không trừ tiền)
      const required = Number(payload.budgetAmount || 0);
      if (payload.budgetType === "Fixed" && required > 0) {
        const balance = await getWalletBalance(currentUserId);
        if (balance < required) {
          alert(
            `${t("NewProject.InsufficientBalance")}\n` +
              `${t("NewProject.Required")}: ${required.toLocaleString("vi-VN")} VND\n` +
              `${t("NewProject.CurrentBalance")}: ${balance.toLocaleString("vi-VN")} VND\n\n` +
              `${t("NewProject.PleaseTopUp")}`
          );
          return; // dừng lại, không tạo project
        }
      }

      // ② TẠO JOB (không rút tiền ở bước này)
      await api.post("/jobs", payload);

      alert(t("NewProject.PostSuccess"));
      // Tạm thời vẫn điều hướng về trang projects (sau này sẽ rename sang jobs)
      navigate("/projects");
    } catch (e) {
      console.error("Create project failed", e?.response?.data || e);
      alert(
        e?.response?.data?.detail ||
          e?.response?.data?.message ||
          t("NewProject.PostFailed")
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="container-ld py-10">
      <h1 className="text-3xl font-semibold text-center">{t("NewProject.Title")}</h1>

      {err && (
        <div className="mt-6 text-sm text-red-700 bg-red-50 border border-red-200 p-3 rounded">
          {err}
        </div>
      )}

      <div className="mt-8 space-y-6">
        {/* Thông tin dự án */}
        <div className="card">
          <div className="card-header p-5 font-semibold">{t("NewProject.ProjectInfo")}</div>
          <div className="card-body grid md:grid-cols-2 gap-4">
            <div>
              <label className="text-sm">{t("NewProject.ProjectTitleLabel")}</label>
              <Input
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder={t("NewProject.TitlePlaceholder")}
              />
            </div>

            <div>
              <label className="text-sm">{t("NewProject.CategoryLabel")}</label>
              <Select
                value={categoryId}
                onChange={(e) => setCategoryId(e.target.value)}
              >
                <option value="">{t("NewProject.SelectCategory")}</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </Select>
            </div>

            <div className="md:col-span-2">
              <label className="text-sm">{t("NewProject.DescriptionLabel")}</label>
              <Textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder={t("NewProject.DescriptionPlaceholder")}
              />
            </div>
          </div>
        </div>

        {/* Ngân sách */}
        <div className="card">
          <div className="card-header p-5 font-semibold">{t("NewProject.Budget")}</div>
          <div className="card-body grid md:grid-cols-2 gap-4">
            <div>
              <label className="text-sm">{t("NewProject.BudgetType")}</label>
              <div className="mt-2 flex gap-6 text-sm">
                <label className="flex items-center gap-2">
                  <input
                    type="radio"
                    name="budgetType"
                    value="Fixed"
                    checked={budgetType === "Fixed"}
                    onChange={(e) => setBudgetType(e.target.value)}
                  />
                  {t("NewProject.FixedPrice")}
                </label>
                <label className="flex items-center gap-2">
                  <input
                    type="radio"
                    name="budgetType"
                    value="Hourly"
                    checked={budgetType === "Hourly"}
                    onChange={(e) => setBudgetType(e.target.value)}
                  />
                  {t("NewProject.HourlyRate")}
                </label>
              </div>
            </div>

            <div>
              <label className="text-sm">{t("NewProject.TotalBudget")}</label>
              <Input
                type="number"
                value={budgetAmount}
                onChange={(e) => setBudgetAmount(Number(e.target.value))}
                min={0}
              />
            </div>
          </div>
        </div>

        {/* Thời gian */}
        <div className="card">
          <div className="card-header p-5 font-semibold">{t("NewProject.Time")}</div>
          <div className="card-body grid md:grid-cols-2 gap-4">
            <div>
              <label className="text-sm">{t("NewProject.Deadline")}</label>
              <Input
                type="date"
                value={deadline}
                onChange={(e) => setDeadline(e.target.value)}
              />
            </div>
          </div>
        </div>

        {/* Kỹ năng */}
        <div className="card">
          <div className="card-header p-5 font-semibold">{t("NewProject.RequiredSkills")}</div>
          <div className="card-body grid md:grid-cols-3 gap-3 text-sm">
            {skillOptions.map((s) => (
              <label key={s.id} className="flex items-center gap-2">
                <input
                  type="checkbox"
                  checked={skills.includes(s.id)}
                  onChange={() => toggleSkill(s.id)}
                />
                {s.name}
              </label>
            ))}
          </div>
        </div>

        {/* Hình ảnh dự án */}
        <div className="card">
          <div className="card-header p-5 font-semibold">{t("NewProject.ProjectImages")}</div>
          <div className="card-body">
            <ImageUpload
              multiple={true}
              folder="projects"
              onUploadSuccess={(urls) => {
                if (Array.isArray(urls)) {
                  setImages(prev => [...prev, ...urls]);
                }
              }}
            />
            {images.length > 0 && (
              <div className="mt-4 grid grid-cols-3 gap-3">
                {images.map((url, index) => (
                  <div key={index} className="relative group">
                    <img
                      src={url}
                      alt={`Project ${index + 1}`}
                      className="w-full h-32 object-cover rounded"
                    />
                    <button
                      onClick={() => setImages(images.filter((_, i) => i !== index))}
                      className="absolute top-1 right-1 bg-red-500 text-white rounded-full w-6 h-6 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity"
                    >
                      ×
                    </button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Actions */}
        <div className="flex justify-end gap-3">
          <Button
            variant="outline"
            onClick={() => navigate("/projects")}
            disabled={submitting}
          >
            {t("NewProject.Cancel")}
          </Button>
          <Button onClick={handleSubmit} disabled={submitting}>
            {submitting ? t("NewProject.Posting") : t("NewProject.PostProject")}
          </Button>
        </div>
      </div>
    </div>
  );
}
