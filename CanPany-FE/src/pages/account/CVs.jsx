import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import apiService from "../../lib/api.service";
import { jwtDecode } from "jwt-decode";
import Button from "../../components/ui/button";
import Spinner from "../../components/Spinner";
import { Card } from "../../components/ui/card";
import Badge from "../../components/ui/badge";
import { toast } from "sonner";
import { i18n } from "../../services/i18n.service";

export default function CVs() {
  const navigate = useNavigate();
  const [cvs, setCvs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [currentUserId, setCurrentUserId] = useState(null);

  useEffect(() => {
    const token = localStorage.getItem("token") || sessionStorage.getItem("token");
    if (!token) {
      navigate("/login");
      return;
    }

    try {
      const decoded = jwtDecode(token);
      setCurrentUserId(decoded.sub || decoded.nameid || decoded.userId);
    } catch (e) {
      console.error("Error decoding token:", e);
      navigate("/login");
    }
  }, [navigate]);

  useEffect(() => {
    if (currentUserId) {
      loadCVs();
    }
  }, [currentUserId]);

  const loadCVs = async () => {
    try {
      setLoading(true);
      const data = await apiService.get("/cvs/my");
      setCvs(data || []);
    } catch (error) {
      console.error("Error loading CVs:", error);
      toast.error(i18n.t("CV.LoadError") || "Không thể tải danh sách CV");
    } finally {
      setLoading(false);
    }
  };

  const handleFileUpload = async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    if (file.size > 10 * 1024 * 1024) {
      toast.error(i18n.t("CV.FileTooLarge") || "File quá lớn (tối đa 10MB)");
      return;
    }

    try {
      setUploading(true);
      const formData = new FormData();
      formData.append("file", file);

      await apiService.upload("/cvs", formData);

      toast.success(i18n.t("CV.UploadSuccess") || "Đã tải lên CV thành công");
      loadCVs();
    } catch (error) {
      console.error("Error uploading CV:", error);
      toast.error(error?.message || error?.response?.data?.message || i18n.t("CV.UploadError") || "Không thể tải lên CV");
    } finally {
      setUploading(false);
      e.target.value = ""; // Reset input
    }
  };

  const handleSetPrimary = async (cvId) => {
    try {
      await apiService.post(`/cvs/${cvId}/set-primary`);
      toast.success(i18n.t("CV.SetPrimarySuccess") || "Đã đặt CV chính");
      loadCVs();
    } catch (error) {
      console.error("Error setting primary CV:", error);
      toast.error(error?.message || error?.response?.data?.message || "Không thể đặt CV chính");
    }
  };

  const handleAnalyze = async (cvId) => {
    try {
      toast.info(i18n.t("CV.Analyzing") || "Đang phân tích CV...");
      await apiService.post(`/cvs/${cvId}/analyze`);
      toast.success(i18n.t("CV.AnalysisComplete") || "Phân tích CV hoàn tất");
      loadCVs();
    } catch (error) {
      console.error("Error analyzing CV:", error);
      toast.error(error?.message || error?.response?.data?.message || "Không thể phân tích CV");
    }
  };

  const handleDelete = async (cvId) => {
    if (!confirm(i18n.t("CV.ConfirmDelete") || "Bạn có chắc chắn muốn xóa CV này?")) return;

    try {
      await apiService.delete(`/cvs/${cvId}`);
      toast.success(i18n.t("CV.DeleteSuccess") || "Đã xóa CV");
      loadCVs();
    } catch (error) {
      console.error("Error deleting CV:", error);
      toast.error(error?.message || error?.response?.data?.message || "Không thể xóa CV");
    }
  };

  if (loading) {
    return (
      <div className="container-ld py-24">
        <Spinner />
      </div>
    );
  }

  return (
    <div className="container-ld py-8">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold mb-2">{i18n.t("CV.MyCVs")}</h1>
          <p className="text-gray-600">{i18n.t("CV.ManageDescription") || "Quản lý CV của bạn"}</p>
        </div>
        <label className="cursor-pointer">
          <input
            type="file"
            accept=".pdf,.doc,.docx"
            onChange={handleFileUpload}
            disabled={uploading}
            className="hidden"
          />
          <Button disabled={uploading}>
            {uploading ? i18n.t("Common.Uploading") || "Đang tải..." : i18n.t("CV.Upload")}
          </Button>
        </label>
      </div>

      {cvs.length === 0 ? (
        <Card className="p-12 text-center">
          <p className="text-gray-500 mb-4">{i18n.t("CV.NoCVs") || "Bạn chưa có CV nào"}</p>
          <label className="cursor-pointer inline-block">
            <input
              type="file"
              accept=".pdf,.doc,.docx"
              onChange={handleFileUpload}
              disabled={uploading}
              className="hidden"
            />
            <Button>{i18n.t("CV.Upload")}</Button>
          </label>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {cvs.map((cv) => (
            <Card key={cv.id} className="p-6">
              <div className="flex items-start justify-between mb-4">
                <div className="flex-1">
                  <h3 className="text-lg font-semibold mb-1">{cv.fileName || "CV"}</h3>
                  <div className="flex items-center gap-2 mb-2">
                    {cv.isPrimary && (
                      <Badge variant="default">{i18n.t("CV.Primary")}</Badge>
                    )}
                    {cv.ATSScore && (
                      <Badge variant="outline">
                        ATS: {cv.ATSScore.toFixed(1)}%
                      </Badge>
                    )}
                  </div>
                  <p className="text-sm text-gray-600">
                    {new Date(cv.createdAt).toLocaleDateString("vi-VN")}
                  </p>
                  {cv.extractedSkills && cv.extractedSkills.length > 0 && (
                    <div className="mt-2">
                      <p className="text-xs text-gray-500 mb-1">{i18n.t("CV.Skills") || "Kỹ năng"}:</p>
                      <div className="flex flex-wrap gap-1">
                        {cv.extractedSkills.slice(0, 5).map((skill, idx) => (
                          <Badge key={idx} variant="secondary" className="text-xs">
                            {skill}
                          </Badge>
                        ))}
                        {cv.extractedSkills.length > 5 && (
                          <span className="text-xs text-gray-500">
                            +{cv.extractedSkills.length - 5}
                          </span>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              </div>

              <div className="flex gap-2 mt-4">
                {cv.fileUrl && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => window.open(cv.fileUrl, "_blank")}
                  >
                    {i18n.t("CV.View") || "Xem"}
                  </Button>
                )}
                {!cv.isPrimary && (
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handleSetPrimary(cv.id)}
                  >
                    {i18n.t("CV.SetPrimary") || "Đặt làm chính"}
                  </Button>
                )}
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => handleAnalyze(cv.id)}
                >
                  {i18n.t("CV.Analyze")}
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => handleDelete(cv.id)}
                >
                  {i18n.t("Common.Delete")}
                </Button>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}




