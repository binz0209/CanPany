// src/pages/Projects.jsx
import { useEffect, useState, useCallback, useMemo, useRef } from "react";
import { useNavigate } from "react-router-dom";
import SearchBar from "../../components/SearchBar";
import StatCard from "../../components/StatCard";
import Progress from "../../components/ui/progress";
import Button from "../../components/ui/button";
import apiService from "../../lib/api.service";
import { jwtDecode } from "jwt-decode";
import Spinner from "../../components/Spinner";
import { useI18n } from "../../hooks/useI18n";

export default function Projects() {
  const { t } = useI18n();
  const [projects, setProjects] = useState([]);
  const [statsBase, setStatsBase] = useState([]);
  const [categories, setCategories] = useState([]);
  const [skills, setSkills] = useState([]); // skills từ DB
  const [currentUserId, setCurrentUserId] = useState(null);

  const [editing, setEditing] = useState(null); // { ...project, skillIds: [] } | null
  const [saving, setSaving] = useState(false);
  const [errors, setErrors] = useState({});
  const [apiError, setApiError] = useState("");

  const [activeFilter, setActiveFilter] = useState("ALL");
  const [loading, setLoading] = useState(true);

  const [filterCategory, setFilterCategory] = useState("");
  const [searchText, setSearchText] = useState("");
  const [sortOrder, setSortOrder] = useState("newest");
  const [recommendedProjects, setRecommendedProjects] = useState([]);
  const [loadingRecommended, setLoadingRecommended] = useState(false);

  const [viewing, setViewing] = useState(null);

  const [confirmJob, setConfirmJob] = useState(null);

  const [currentUserName, setCurrentUserName] = useState("");
  
  // Infinite scroll
  const [displayedCount, setDisplayedCount] = useState(10);
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  const scrollContainerRef = useRef(null);
  const ITEMS_PER_PAGE = 10;
  // ===== current user =====
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
        null;
      const name =
        dec.name ||
        dec.username ||
        dec.unique_name ||
        dec.preferred_username ||
        dec.email || // fallback email
        null;
      setCurrentUserId(id);
      setCurrentUserName(name);
    } catch (e) {
      console.error("Decode token error:", e);
    }
  }, []);

  // ====== filter client-side theo search + category + status ======
  const filteredProjects = useMemo(() => {
    // Nếu sortOrder là "related", sử dụng recommendedProjects
    if (sortOrder === "related") {
      let list = [...recommendedProjects];

      // Vẫn áp dụng filter
      list = list.filter((p) => {
        // Filter by status (activeFilter)
        if (activeFilter !== "ALL") {
          const status = p.status || p.Status || "";
          if (status !== activeFilter) return false;
        }

        // Filter by category
        if (filterCategory && p.categoryId !== filterCategory) return false;

        // Filter by search text
        if (searchText.trim()) {
          const txt = removeDiacritics(searchText);
          const title = removeDiacritics(p.title || "");
          const desc = removeDiacritics(p.description || "");
          if (!title.includes(txt) && !desc.includes(txt)) return false;
        }
        return true;
      });

      // Sort by similarity (cao nhất trước)
      return list.sort((a, b) => (b.similarity || 0) - (a.similarity || 0));
    }

    // Sort bình thường
    const getTime = (p) => {
      const t = new Date(p.createdAt || p.updatedAt || 0).getTime();
      return Number.isFinite(t) ? t : 0;
    };

    const list = projects.filter((p) => {
      // Filter by status (activeFilter)
      if (activeFilter !== "ALL") {
        const status = p.status || p.Status || "";
        if (status !== activeFilter) return false;
      }

      // Filter by category
      if (filterCategory && p.categoryId !== filterCategory) return false;

      // Filter by search text
      if (searchText.trim()) {
        const txt = removeDiacritics(searchText);
        const title = removeDiacritics(p.title || "");
        const desc = removeDiacritics(p.description || "");
        if (!title.includes(txt) && !desc.includes(txt)) return false;
      }
      return true;
    });

    // sort by date
    return list.sort((a, b) =>
      sortOrder === "newest" ? getTime(b) - getTime(a) : getTime(a) - getTime(b)
    );
  }, [projects, recommendedProjects, activeFilter, filterCategory, searchText, sortOrder]);

  // Reset displayed count when filter changes
  useEffect(() => {
    setDisplayedCount(ITEMS_PER_PAGE);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeFilter, filterCategory, searchText, sortOrder]);

  // Displayed projects (for pagination)
  const displayedProjects = useMemo(() => {
    return filteredProjects.slice(0, displayedCount);
  }, [filteredProjects, displayedCount]);

  // Infinite scroll handler
  useEffect(() => {
    const handleScroll = () => {
      if (isLoadingMore) return;
      if (displayedCount >= filteredProjects.length) return;

      const scrollContainer = scrollContainerRef.current || window;
      const scrollTop = scrollContainer === window 
        ? window.scrollY 
        : scrollContainer.scrollTop;
      const scrollHeight = scrollContainer === window 
        ? document.documentElement.scrollHeight 
        : scrollContainer.scrollHeight;
      const clientHeight = scrollContainer === window 
        ? window.innerHeight 
        : scrollContainer.clientHeight;

      // Load more when 200px from bottom
      if (scrollTop + clientHeight >= scrollHeight - 200) {
        setIsLoadingMore(true);
        setTimeout(() => {
          setDisplayedCount(prev => Math.min(prev + ITEMS_PER_PAGE, filteredProjects.length));
          setIsLoadingMore(false);
        }, 300);
      }
    };

    const container = scrollContainerRef.current || window;
    container.addEventListener('scroll', handleScroll);
    window.addEventListener('scroll', handleScroll);
    
    return () => {
      container.removeEventListener('scroll', handleScroll);
      window.removeEventListener('scroll', handleScroll);
    };
  }, [isLoadingMore, displayedCount, filteredProjects.length]);

  function removeDiacritics(str) {
    return str
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .toLowerCase();
  }

  // ===== categories & skills =====
  useEffect(() => {
    apiService
      .get("/categories")
      .then((data) => setCategories(data || []))
      .catch((e) => console.error(e));
    apiService
      .get("/skills")
      .then((data) => setSkills(data || []))
      .catch((e) => console.error("Skills error:", e));
  }, []);

  // ===== stats base (ALL) =====
  useEffect(() => {
    apiService
      .get("/projects")
      .then((data) => setStatsBase(data || []))
      .catch((err) => console.error("Load all projects for stats error:", err));
  }, []);

  // ===== load all projects (filtering will be done client-side) =====
  useEffect(() => {
    setLoading(true);
    apiService
      .get("/projects")
      .then((data) => setProjects(data || []))
      .catch((err) => console.error("Load projects error:", err))
      .finally(() => setLoading(false));
  }, []);

  // ===== load recommended projects when sortOrder is "related" =====
  useEffect(() => {
    if (sortOrder === "related" && currentUserId) {
      setLoadingRecommended(true);
      apiService
        .get("/projects/recommended?limit=100")
        .then((data) => {
          const items = data || [];
          setRecommendedProjects(
            items.map((item) => ({
              ...item.project,
              similarity: item.similarity,
            }))
          );
        })
        .catch((err) => {
          console.error("Load recommended projects error:", err);
          setRecommendedProjects([]);
        })
        .finally(() => setLoadingRecommended(false));
    } else {
      setRecommendedProjects([]);
    }
  }, [sortOrder, currentUserId]);

  const catName = useCallback(
    (id) => categories.find((c) => c.id === id)?.name || t("Projects.Other"),
    [categories, t]
  );

  // map skillIds -> names
  const skillNameMap = useMemo(() => {
    const map = new Map();
    for (const s of skills) map.set(s.id, s.name);
    return map;
  }, [skills]);

  const namesFromIds = useCallback(
    (ids = []) => ids.map((id) => skillNameMap.get(id)).filter(Boolean),
    [skillNameMap]
  );

  const startEdit = (p) => {
    if (p.ownerId !== currentUserId) return; // chỉ owner được sửa
    setErrors({});
    setApiError("");

    // chuẩn hoá skillIds từ project
    let skillIds = Array.isArray(p.skillIds) ? p.skillIds : [];
    if (!skillIds.length && Array.isArray(p.skillNames) && skills.length) {
      const nameToId = new Map(skills.map((s) => [s.name.toLowerCase(), s.id]));
      skillIds = p.skillNames
        .map((n) => nameToId.get((n || "").toLowerCase()))
        .filter(Boolean);
    }

    setEditing({
      id: p.id,
      ownerId: p.ownerId,
      title: p.title || "",
      description: p.description || "",
      budgetAmount: p.budgetAmount ?? 0,
      status: p.status || "Open",
      categoryId: p.categoryId || "",
      skillIds,
      createdAt: p.createdAt || null,
      updatedAt: p.updatedAt || null,
    });
  };

  // ➕ Tạo mới
  const startCreate = () => {
    if (!currentUserId) {
      alert(t("Common.NotLoggedIn"));
      return;
    }
    setErrors({});
    setApiError("");
    setEditing({
      id: null, // không có id -> save() sẽ POST
      ownerId: currentUserId,
      title: "",
      description: "",
      budgetAmount: 0,
      status: "Open",
      categoryId: "",
      skillIds: [],
      createdAt: null,
      updatedAt: null,
    });
  };

  const closeEdit = useCallback(() => setEditing(null), []);

  const validate = () => {
    const e = {};
    if (!editing.title?.trim()) e.title = t("Projects.PleaseEnterTitle");
    if (!editing.description?.trim()) e.description = t("Projects.PleaseEnterDescription");
    if (
      Number.isNaN(Number(editing.budgetAmount)) ||
      Number(editing.budgetAmount) < 0
    ) {
      e.budgetAmount = t("Projects.InvalidBudget");
    }
    if (!editing.categoryId) e.categoryId = t("Projects.PleaseSelectCategory");
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const save = async () => {
    if (!editing) return;
    if (!validate()) return;

    try {
      setSaving(true);
      setApiError("");

      let savedProject;

      if (!editing.id) {
        // ====== CREATE (POST) ======
        const payloadCreate = {
          ownerId: editing.ownerId,
          title: editing.title,
          description: editing.description,
          budgetAmount: Number(editing.budgetAmount),
          status: editing.status,
          categoryId: editing.categoryId,
        };
        const data = await apiService.post("/projects", payloadCreate);
        savedProject = data ?? payloadCreate;

        // đồng bộ skills
        if (Array.isArray(editing.skillIds)) {
          try {
            await apiService.post("/projectskills/sync", {
              projectId: editing.id || savedProject.id,
              skillIds: editing.skillIds || [],
            });

            // gán skillIds/skillNames vào chính project object để UI render
            savedProject.skillIds = editing.skillIds || [];
            savedProject.skillNames = namesFromIds(savedProject.skillIds);
          } catch (syncErr) {
            console.warn(
              "Sync project skills warning:",
              syncErr?.response || syncErr
            );
          }
        }

        // cập nhật UI
        setProjects((prev) => [{ ...savedProject }, ...prev]);
        setStatsBase((prev) => [{ ...savedProject }, ...prev]);
      } else {
        // ====== UPDATE (PUT) ======
        const payloadUpdate = {
          id: editing.id,
          ownerId: editing.ownerId,
          title: editing.title,
          description: editing.description,
          budgetAmount: Number(editing.budgetAmount),
          status: editing.status,
          categoryId: editing.categoryId,
          updatedAt: new Date().toISOString(),
        };
        const data = await apiService.put(
          `/projects/${editing.id}`,
          payloadUpdate
        );
        savedProject = data ?? payloadUpdate;

        try {
          await apiService.post("/projectskills/sync", {
            projectId: editing.id,
            skillIds: editing.skillIds || [],
          });
          savedProject.skillIds = editing.skillIds || [];
          savedProject.skillNames = namesFromIds(savedProject.skillIds);
        } catch (syncErr) {
          console.warn(
            "Sync project skills (update) warning:",
            syncErr?.response || syncErr
          );
        }

        setProjects((prev) =>
          prev.map((p) => (p.id === editing.id ? { ...p, ...savedProject } : p))
        );
        setStatsBase((prev) =>
          prev.map((p) => (p.id === editing.id ? { ...p, ...savedProject } : p))
        );
      }

      closeEdit();
    } catch (err) {
      const msg =
        err?.response?.data?.detail ||
        err?.response?.data?.message ||
        err?.response?.data ||
        err?.message ||
        t("Common.SaveFailed");
      console.error("Save project error:", err?.response || err);
      setApiError(typeof msg === "string" ? msg : t("Common.CannotSave"));
    } finally {
      setSaving(false);
    }
  };

  // ===== View modal helpers =====
  const openView = (p) => {
    const isIds = Array.isArray(p.skillIds) && p.skillIds.length > 0;
    const resolvedSkills = isIds
      ? namesFromIds(p.skillIds)
      : p.skillNames || [];
    setViewing({
      ...p,
      resolvedSkills,
      categoryName: catName(p.categoryId),
      isOwner: p.ownerId === currentUserId,
    });
  };
  const closeView = () => setViewing(null);

  const countOpen = statsBase.filter((p) => p.status === "Open").length;
  const countInProgress = statsBase.filter(
    (p) => p.status === "InProgress"
  ).length;
  const countCompleted = statsBase.filter(
    (p) => p.status === "Completed"
  ).length;
  const totalBudget =
    (statsBase
      .reduce((sum, p) => sum + (p.budgetAmount || 0), 0)
      .toLocaleString("vi-VN") || "0") + " đ";

  return (
    <div className="container-ld py-8 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">{t("Projects.NeedFreelancer")}</h1>
        <Button onClick={startCreate}>{t("Projects.NewProject")}</Button>
      </div>

      <div className="flex flex-col md:flex-row gap-4 items-center justify-between">
        {/* Search */}
        <input
          type="text"
          className="input w-full md:w-1/2"
          placeholder={t("Projects.SearchPlaceholder")}
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
        />

        <div className="flex gap-3 w-full md:w-auto">
          {/* Filter theo danh mục */}
          <select
            className="select w-full md:w-auto"
            value={filterCategory}
            onChange={(e) => setFilterCategory(e.target.value)}
          >
            <option value="">— Tất cả danh mục —</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>

          {/* NEW: Sort theo ngày */}
          <select
            className="select w-full md:w-auto"
            value={sortOrder}
            onChange={(e) => setSortOrder(e.target.value)}
          >
            <option value="newest">Mới nhất</option>
            <option value="oldest">Cũ nhất</option>
            <option value="related">Liên quan</option>
          </select>
        </div>
      </div>

      {/* Filters clickable */}
      <div className="grid md:grid-cols-5 gap-4">
        <div
          role="button"
          onClick={() => setActiveFilter("ALL")}
          className={`transition rounded-xl ${
            activeFilter === "ALL" ? "ring-2 ring-brand-500" : ""
          }`}
        >
          <StatCard icon={"📦"} label={t("Projects.TotalProjects")} value={statsBase.length} />
        </div>
        <div
          role="button"
          onClick={() => setActiveFilter("Open")}
          className={`transition rounded-xl ${
            activeFilter === "Open" ? "ring-2 ring-brand-500" : ""
          }`}
        >
          <StatCard icon={"🟢"} label={t("Projects.Open")} value={countOpen} />
        </div>
        <div
          role="button"
          onClick={() => setActiveFilter("InProgress")}
          className={`transition rounded-xl ${
            activeFilter === "InProgress" ? "ring-2 ring-brand-500" : ""
          }`}
        >
          <StatCard
            icon={"⏳"}
            label={t("Projects.InProgress")}
            value={countInProgress}
          />
        </div>
        <div
          role="button"
          onClick={() => setActiveFilter("Completed")}
          className={`transition rounded-xl ${
            activeFilter === "Completed" ? "ring-2 ring-brand-500" : ""
          }`}
        >
          <StatCard icon={"✅"} label={t("Projects.Completed")} value={countCompleted} />
        </div>
        <StatCard icon={"💰"} label={t("Projects.TotalBudget")} value={totalBudget} />
      </div>

      {/* List */}
      <div ref={scrollContainerRef}>
      {loading ? (
          <div className="card p-6 flex items-center justify-center gap-3">
            <Spinner />
            <span>{t("Common.Loading")}</span>
          </div>
      ) : filteredProjects.length === 0 ? (
        <div className="card p-6">{t("Common.NoData")}</div>
      ) : (
          <>
            {displayedProjects.map((p) => {
          const isOwner = p.ownerId === currentUserId;
          const shownSkills =
            Array.isArray(p.skillIds) && p.skillIds.length
              ? namesFromIds(p.skillIds)
              : p.skillNames || [];
          return (
            <div key={p.id} className="card">
              <div className="card-body">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-start justify-between gap-3 mb-2">
                      <div className="text-lg font-semibold flex-1">{p.title}</div>
                      {sortOrder === "related" && p.similarity !== undefined && (
                        <div className="flex-shrink-0">
                          <div className="px-3 py-1.5 bg-green-100 text-green-700 rounded-full text-sm font-semibold whitespace-nowrap">
                            {p.similarity}% phù hợp
                          </div>
                        </div>
                      )}
                    </div>
                    <div className="mt-1 text-sm text-slate-500">
                      {p.description}
                    </div>
                    <div className="mt-2 flex gap-2 flex-wrap">
                      <span className="badge">{catName(p.categoryId)}</span>
                      {shownSkills.slice(0, 6).map((s) => (
                        <span key={s} className="badge badge-outline">
                          {s}
                        </span>
                      ))}
                    </div>
                  </div>
                  <div className="text-right min-w-[140px] ml-4">
                    <div className="text-xs uppercase tracking-wide text-slate-500">
                      {t("Projects.BudgetLabel")}
                    </div>
                    <div className="text-brand-700 font-semibold">
                      {p.budgetAmount?.toLocaleString("vi-VN") ?? "—"} đ
                    </div>
                  </div>
                </div>
                <div className="mt-4 flex items-center justify-between text-sm">
                  <div className="flex items-center gap-2">
                    <span>{t("Projects.StatusLabel")}:</span>
                    <span
                      className={`font-semibold ${
                        p.status === "Open"
                          ? "text-green-600"
                          : p.status === "InProgress"
                          ? "text-blue-600"
                          : p.status === "Completed"
                          ? "text-gray-600"
                          : "text-red-600"
                      }`}
                    >
                      {p.status}
                    </span>
                  </div>
                  <div className="text-xs text-slate-500">
                    Đăng lúc:{" "}
                    {p.createdAt
                      ? new Date(p.createdAt).toLocaleString("vi-VN")
                      : "—"}
                  </div>
                </div>

                <div className="mt-4 flex gap-2">
                  <Button variant="outline" onClick={() => openView(p)}>
                    Xem
                  </Button>
                  {isOwner ? (
                    <Button variant="outline" onClick={() => startEdit(p)}>
                      Chỉnh sửa
                    </Button>
                  ) : (
                    p.status === "Open" && (
                      <Button
                        variant="outline"
                        onClick={() => setConfirmJob(p)}
                      >
                        Nhận job
                      </Button>
                    )
                  )}
                </div>
              </div>
            </div>
          );
        })}

        {/* Loading more indicator */}
        {isLoadingMore && (
          <div className="flex items-center justify-center py-4">
            <Spinner size="md" />
          </div>
        )}

        {/* End of list indicator */}
        {!loading && displayedCount >= filteredProjects.length && filteredProjects.length > 0 && (
          <div className="text-center py-4 text-slate-500 text-sm">
            Đã hiển thị tất cả {filteredProjects.length} dự án
          </div>
        )}
          </>
        )}
      </div>
      {/* ===== Modal xác nhận nhận job ===== */}
      {confirmJob && (
        <>
          <div
            className="fixed inset-0 bg-black/40 backdrop-blur-sm z-40"
            onClick={() => setConfirmJob(null)}
          />
          <div className="fixed left-1/2 top-1/2 z-50 w-[min(96vw,480px)] -translate-x-1/2 -translate-y-1/2">
            <div className="rounded-2xl bg-white shadow-2xl border border-slate-200">
              {/* Header */}
              <div className="p-4 border-b flex items-center justify-between">
                <div className="font-semibold">Ứng tuyển dự án</div>
                <button
                  className="btn btn-sm btn-ghost"
                  onClick={() => setConfirmJob(null)}
                >
                  ✕
                </button>
              </div>

              {/* Body */}
              <div className="p-5 space-y-4">
                <p>
                  Bạn đang ứng tuyển vào dự án <b>{confirmJob.title}</b>
                </p>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Cover Letter</label>
                  <textarea
                    className="textarea textarea-bordered w-full"
                    rows={3}
                    value={confirmJob.coverLetter || ""}
                    onChange={(e) =>
                      setConfirmJob({
                        ...confirmJob,
                        coverLetter: e.target.value,
                      })
                    }
                    placeholder={t("Profile.BioPlaceholder")}
                  />
                </div>

                <div className="space-y-2">
                  <label className="text-sm font-medium">Bid Amount</label>
                  <input
                    type="number"
                    className="input input-bordered w-full"
                    value={confirmJob.bidAmount || ""}
                    onChange={(e) =>
                      setConfirmJob({
                        ...confirmJob,
                        bidAmount: e.target.value,
                      })
                    }
                    placeholder={`Theo budget của dự án: ${
                      confirmJob.budgetAmount || "0"
                    }$`}
                  />
                </div>
              </div>

              {/* Footer */}
              <div className="p-4 border-t flex items-center justify-end gap-2">
                <Button variant="outline" onClick={() => setConfirmJob(null)}>
                  {t("Common.Cancel")}
                </Button>
                <Button
                  onClick={async () => {
                    try {
                      const cover =
                        (confirmJob.coverLetter || "").trim() ||
                        t("Projects.InterestedInProject");

                      // nếu bỏ trống bid -> theo budgetAmount của project
                      const numericBid =
                        confirmJob.bidAmount === "" ||
                        confirmJob.bidAmount == null
                          ? confirmJob.budgetAmount ?? null
                          : Number(confirmJob.bidAmount);

                      const bid =
                        Number.isFinite(numericBid) && numericBid > 0
                          ? numericBid
                          : confirmJob.budgetAmount ?? null;

                      // 1) Tạo proposal
                      const proposalPayload = {
                        projectId: confirmJob.id || confirmJob._id,
                        freelancerId: currentUserId,
                        coverLetter: cover,
                        bidAmount: bid, // có thể null -> BE hiểu là theo budget
                        status: "Pending",
                        createdAt: new Date().toISOString(),
                      };
                      console.log("Proposal payload:", proposalPayload);

                      const createdProposal = await apiService.post(
                        "/proposals",
                        proposalPayload
                      );

                      // ID trả về có thể là id hoặc Id tuỳ config JSON
                      const createdProposalId =
                        createdProposal?.id || createdProposal?.Id;

                      // 2) Tạo message “proposal”
                      const messagePayload = {
                        projectId: confirmJob.id || confirmJob._id,
                        proposalId: createdProposalId,
                        clientId: confirmJob.ownerId,
                        freelancerId: currentUserId,
                        projectTitle: confirmJob.title,
                        clientName: confirmJob.ownerName || "", // nếu có
                        freelancerName: currentUserName || "",
                        coverLetter: cover,
                        bidAmount: bid,
                      };
                      console.log("Message(proposal) payload:", messagePayload);

                      await apiService.post("/messages/proposal", messagePayload);

                      // alert("Đã gửi proposal và tin nhắn tới chủ dự án.");
                    } catch (err) {
                      console.error("Proposal error:", err?.response || err);
                      alert(
                        err?.response?.data?.detail ||
                          t("Projects.CannotSendProposal")
                      );
                    } finally {
                      setConfirmJob(null);
                    }
                  }}
                >
                  Gửi proposal
                </Button>
              </div>
            </div>
          </div>
        </>
      )}

      {/* ===== Modal tạo/sửa ===== */}
      {editing && (
        <>
          <div
            className="fixed inset-0 bg-black/40 backdrop-blur-sm z-40"
            onClick={closeEdit}
          />
          <div className="fixed left-1/2 top-1/2 z-50 w-[min(96vw,760px)] -translate-x-1/2 -translate-y-1/2">
            <div className="rounded-2xl bg-white shadow-2xl border border-slate-200">
              <div className="p-4 border-b flex items-center justify-between">
                <div className="font-semibold">
                  {editing.id ? t("Projects.EditProject") : t("Projects.CreateProject")}
                </div>
                <button className="btn btn-sm btn-ghost" onClick={closeEdit}>
                  ✕
                </button>
              </div>

              <div className="p-5 space-y-4">
                {apiError && (
                  <div className="text-sm text-red-600 border border-red-200 bg-red-50 p-2 rounded">
                    {apiError}
                  </div>
                )}

                <div className="grid md:grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm font-medium">{t("Projects.TitleLabel")}</label>
                    <input
                      className={`input mt-1 w-full ${
                        errors.title ? "border-red-500" : ""
                      }`}
                      value={editing.title}
                      onChange={(e) =>
                        setEditing((s) => ({ ...s, title: e.target.value }))
                      }
                      placeholder={t("Projects.TitlePlaceholder")}
                    />
                    {errors.title && (
                      <p className="text-xs text-red-600 mt-1">
                        {errors.title}
                      </p>
                    )}
                  </div>

                  <div>
                    <label className="text-sm font-medium">{t("Projects.CategoryLabel")}</label>
                    <select
                      className={`select mt-1 w-full ${
                        errors.categoryId ? "border-red-500" : ""
                      }`}
                      value={editing.categoryId}
                      onChange={(e) =>
                        setEditing((s) => ({
                          ...s,
                          categoryId: e.target.value,
                        }))
                      }
                    >
                      <option value="">{t("Projects.SelectCategory")}</option>
                      {categories.map((c) => (
                        <option key={c.id} value={c.id}>
                          {c.name}
                        </option>
                      ))}
                    </select>
                    {errors.categoryId && (
                      <p className="text-xs text-red-600 mt-1">
                        {errors.categoryId}
                      </p>
                    )}
                  </div>
                </div>

                <div>
                  <label className="text-sm font-medium">{t("Projects.DescriptionLabel")}</label>
                  <textarea
                    rows={4}
                    className={`textarea mt-1 w-full ${
                      errors.description ? "border-red-500" : ""
                    }`}
                    value={editing.description}
                    onChange={(e) =>
                      setEditing((s) => ({ ...s, description: e.target.value }))
                    }
                    placeholder={t("Projects.DescriptionPlaceholder")}
                  />
                  {errors.description && (
                    <p className="text-xs text-red-600 mt-1">
                      {errors.description}
                    </p>
                  )}
                </div>

                <div className="grid md:grid-cols-3 gap-4">
                  <div>
                    <label className="text-sm font-medium">{t("Projects.BudgetLabel")}</label>
                    <input
                      type="number"
                      className={`input mt-1 w-full ${
                        errors.budgetAmount ? "border-red-500" : ""
                      }`}
                      value={editing.budgetAmount}
                      onChange={(e) =>
                        setEditing((s) => ({
                          ...s,
                          budgetAmount: e.target.value,
                        }))
                      }
                      placeholder="0"
                      min={0}
                    />
                    {errors.budgetAmount && (
                      <p className="text-xs text-red-600 mt-1">
                        {errors.budgetAmount}
                      </p>
                    )}
                  </div>

                  <div>
                    <label className="text-sm font-medium">{t("Projects.StatusLabel")}</label>
                    <select
                      className="select mt-1 w-full"
                      value={editing.status}
                      onChange={(e) =>
                        setEditing((s) => ({ ...s, status: e.target.value }))
                      }
                    >
                      <option value="Open">Open</option>
                      <option value="InProgress">InProgress</option>
                      <option value="Completed">Completed</option>
                      <option value="Closed">Closed</option>
                    </select>
                  </div>

                  {/* === Multi-select Skills từ DB === */}
                  <div className="md:col-span-1">
                    <label className="text-sm font-medium">{t("Projects.SkillsLabel")}</label>
                    <SkillMultiSelect
                      skills={skills}
                      value={editing.skillIds}
                      onChange={(ids) =>
                        setEditing((s) => ({ ...s, skillIds: ids }))
                      }
                    />
                  </div>
                </div>
              </div>

              <div className="p-4 border-t flex items-center justify-end gap-2">
                <Button variant="outline" onClick={closeEdit} disabled={saving}>
                  {t("Common.Cancel")}
                </Button>
                <Button onClick={save} disabled={saving}>
                  {saving
                    ? t("Projects.Saving")
                    : editing.id
                    ? t("Projects.SaveChanges")
                    : t("Projects.CreateProjectButton")}
                </Button>
              </div>
            </div>
          </div>
        </>
      )}

      {/* ===== Modal XEM CHI TIẾT ===== */}
      {viewing && (
        <>
          <div
            className="fixed inset-0 bg-black/40 backdrop-blur-sm z-40"
            onClick={closeView}
          />
          <div className="fixed left-1/2 top-1/2 z-50 w-[min(96vw,760px)] -translate-x-1/2 -translate-y-1/2">
            <div className="rounded-2xl bg-white shadow-2xl border border-slate-200">
              <div className="p-4 border-b flex items-center justify-between">
                <div className="font-semibold">Chi tiết dự án</div>
                <button className="btn btn-sm btn-ghost" onClick={closeView}>
                  ✕
                </button>
              </div>

              <div className="p-5 space-y-4 max-h-[70vh] overflow-y-auto">
                {/* Hình ảnh dự án */}
                {viewing.images && viewing.images.length > 0 && (
                  <div className="mb-4">
                    <div className="text-sm font-semibold mb-2">Hình ảnh dự án</div>
                    <div className="grid grid-cols-2 gap-2">
                      {viewing.images.map((imgUrl, index) => (
                        <img
                          key={index}
                          src={imgUrl}
                          alt={`Project image ${index + 1}`}
                          className="w-full h-32 object-cover rounded cursor-pointer hover:opacity-90"
                          onClick={() => window.open(imgUrl, '_blank')}
                        />
                      ))}
                    </div>
                  </div>
                )}

                <div className="flex items-start justify-between gap-4">
                  <div>
                    <div className="text-xl font-semibold">{viewing.title}</div>
                    <div className="mt-2 text-sm text-slate-600">
                      {viewing.description}
                    </div>

                    <div className="mt-3 flex flex-wrap gap-2">
                      <span className="badge">{viewing.categoryName}</span>
                      {(viewing.resolvedSkills || []).map((s) => (
                        <span key={s} className="badge badge-outline">
                          {s}
                        </span>
                      ))}
                    </div>
                  </div>

                  <div className="text-right min-w-[160px]">
                    <div className="text-xs uppercase tracking-wide text-slate-500">
                      {t("Projects.BudgetLabel")}
                    </div>
                    <div className="text-brand-700 font-semibold">
                      {viewing.budgetAmount?.toLocaleString("vi-VN") ?? "—"} đ
                    </div>
                    <div className="mt-2 text-xs text-slate-600">
                      {t("Projects.StatusLabel")}:{" "}
                      <span className="badge badge-outline">
                        {viewing.status}
                      </span>
                    </div>
                    <div className="mt-1 text-xs text-slate-500">
                      Đăng lúc:{" "}
                      {viewing.createdAt
                        ? new Date(viewing.createdAt).toLocaleString("vi-VN")
                        : "—"}
                    </div>
                  </div>
                </div>
              </div>

              <div className="p-4 border-t flex items-center justify-end gap-2">
                {viewing.isOwner ? (
                  <Button
                    variant="outline"
                    onClick={() => {
                      closeView();
                      startEdit(viewing);
                    }}
                  >
                    Chỉnh sửa
                  </Button>
                ) : (
                  viewing.status === "Open" && (
                    <Button
                      variant="outline"
                      onClick={() =>
                        setConfirmJob({
                          projectId: p.id,
                          title: p.title,
                          clientId: p.ownerId,
                        })
                      }
                    >
                      Nhận job
                    </Button>
                  )
                )}
                <Button onClick={closeView}>Đóng</Button>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

/* ================== Mini Skill Multi-Select ================== */
function SkillMultiSelect({ skills, value = [], onChange }) {
  const [q, setQ] = useState("");
  const selected = new Set(value);

  const filtered = useMemo(() => {
    const s = (q || "").trim().toLowerCase();
    if (!s) return skills;
    return skills.filter((it) => (it.name || "").toLowerCase().includes(s));
  }, [q, skills]);

  const toggle = (id) => {
    const next = new Set(selected);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    onChange(Array.from(next));
  };

  return (
    <div className="mt-1 w-full">
      <input
        className="input w-full mb-2"
        placeholder={t("Projects.SearchSkillsPlaceholder")}
        value={q}
        onChange={(e) => setQ(e.target.value)}
      />
      <div className="border rounded-lg h-[160px] overflow-auto p-2 space-y-1 bg-slate-50">
        {filtered.length === 0 ? (
          <div className="text-xs text-slate-500 px-1">
            Không có kỹ năng phù hợp
          </div>
        ) : (
          filtered.map((s) => (
            <label
              key={s.id}
              className="flex items-center gap-2 px-2 py-1 rounded hover:bg-white cursor-pointer"
            >
              <input
                type="checkbox"
                className="checkbox"
                checked={selected.has(s.id)}
                onChange={() => toggle(s.id)}
              />
              <span className="text-sm">{s.name}</span>
            </label>
          ))
        )}
      </div>
      {value?.length > 0 && (
        <div className="mt-2 flex flex-wrap gap-2">
          {skills
            .filter((s) => value.includes(s.id))
            .map((s) => (
              <span key={s.id} className="badge badge-outline">
                {s.name}
              </span>
            ))}
        </div>
      )}
    </div>
  );
}
