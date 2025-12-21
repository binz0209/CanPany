import { useEffect, useState } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import apiService from "../lib/api.service";
import { jwtDecode } from "jwt-decode";
import Button from "../components/ui/button";
import Spinner from "../components/Spinner";
import { toast } from "sonner";
import Badge from "../components/ui/badge";
import { Card } from "../components/ui/card";
import Textarea from "../components/ui/textarea";
import Input from "../components/ui/input";

export default function ProjectDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [project, setProject] = useState(null);
  const [owner, setOwner] = useState(null);
  const [loading, setLoading] = useState(true);
  const [currentUserId, setCurrentUserId] = useState(null);
  const [isOwner, setIsOwner] = useState(false);
  const [proposals, setProposals] = useState([]);
  const [loadingProposals, setLoadingProposals] = useState(false);
  
  // Proposal form
  const [showProposalForm, setShowProposalForm] = useState(false);
  const [proposalData, setProposalData] = useState({
    coverLetter: "",
    bidAmount: "",
  });
  const [submittingProposal, setSubmittingProposal] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem("token") || sessionStorage.getItem("token");
    if (token) {
      try {
        const decoded = jwtDecode(token);
        setCurrentUserId(decoded.sub || decoded.nameid || decoded.userId);
      } catch (e) {
        console.error("Error decoding token:", e);
      }
    }
  }, []);

  useEffect(() => {
    if (id) {
      loadProject();
    }
  }, [id]);

  useEffect(() => {
    if (project && currentUserId) {
      setIsOwner(project.ownerId === currentUserId);
      if (isOwner) {
        loadProposals();
      }
    }
  }, [project, currentUserId, isOwner]);

  const loadProject = async () => {
    try {
      setLoading(true);
      const data = await apiService.get(`/projects/${id}`);
      setProject(data);
      
      // Load owner info
      if (data.ownerId) {
        try {
          const ownerData = await apiService.get(`/users/${data.ownerId}`);
          setOwner(ownerData);
        } catch (e) {
          console.error("Error loading owner:", e);
        }
      }
    } catch (error) {
      console.error("Error loading project:", error);
      toast.error("Không thể tải thông tin dự án");
      navigate("/projects");
    } finally {
      setLoading(false);
    }
  };

  const loadProposals = async () => {
    try {
      setLoadingProposals(true);
      const data = await apiService.get(`/proposals/by-project/${id}`);
      setProposals(data || []);
    } catch (error) {
      console.error("Error loading proposals:", error);
    } finally {
      setLoadingProposals(false);
    }
  };

  const handleSubmitProposal = async (e) => {
    e.preventDefault();
    if (!currentUserId) {
      toast.error("Vui lòng đăng nhập để gửi đề xuất");
      navigate("/login");
      return;
    }

    try {
      setSubmittingProposal(true);
      await apiService.post("/proposals", {
        projectId: id,
        freelancerId: currentUserId,
        coverLetter: proposalData.coverLetter,
        bidAmount: parseFloat(proposalData.bidAmount),
      });

      toast.success("Đã gửi đề xuất thành công!");
      setShowProposalForm(false);
      setProposalData({ coverLetter: "", bidAmount: "" });
      
      // Reload proposals if owner
      if (isOwner) {
        loadProposals();
      }
    } catch (error) {
      console.error("Error submitting proposal:", error);
      toast.error(error?.message || error?.response?.data?.message || "Không thể gửi đề xuất");
    } finally {
      setSubmittingProposal(false);
    }
  };

  const handleAcceptProposal = async (proposalId) => {
    if (!confirm("Bạn có chắc chắn muốn chấp nhận đề xuất này?")) return;

    try {
      const data = await apiService.post(`/proposals/${proposalId}/accept`);
      toast.success("Đã chấp nhận đề xuất! Hợp đồng đã được tạo.");
      
      // Navigate to contract if contractId is returned
      if (data?.contractId) {
        navigate(`/contracts/${data.contractId}`);
      } else {
        loadProposals();
        loadProject();
      }
    } catch (error) {
      console.error("Error accepting proposal:", error);
      toast.error(error?.message || error?.response?.data?.message || "Không thể chấp nhận đề xuất");
    }
  };

  if (loading) {
    return (
      <div className="container-ld py-24">
        <Spinner />
      </div>
    );
  }

  if (!project) {
    return (
      <div className="container-ld py-24">
        <div className="text-center">
          <h2 className="text-2xl font-bold mb-4">Không tìm thấy dự án</h2>
          <Button onClick={() => navigate("/projects")}>Quay lại danh sách</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="container-ld py-8">
      <div className="mb-6">
        <Button variant="outline" onClick={() => navigate("/projects")}>
          ← Quay lại
        </Button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Main Content */}
        <div className="lg:col-span-2 space-y-6">
          <Card className="p-6">
            <div className="flex items-start justify-between mb-4">
              <div>
                <h1 className="text-3xl font-bold mb-2">{project.title}</h1>
                <div className="flex items-center gap-2 text-gray-600">
                  <span>Bởi</span>
                  <Link
                    to={`/profile/${project.ownerId}`}
                    className="font-semibold text-blue-600 hover:underline"
                  >
                    {owner?.fullName || "Người dùng"}
                  </Link>
                  <span>•</span>
                  <span>{new Date(project.createdAt).toLocaleDateString("vi-VN")}</span>
                </div>
              </div>
              <Badge variant={project.status === "Open" ? "default" : "secondary"}>
                {project.status}
              </Badge>
            </div>

            <div className="prose max-w-none mb-6">
              <p className="text-gray-700 whitespace-pre-wrap">{project.description}</p>
            </div>

            {project.budgetAmount && (
              <div className="border-t pt-4">
                <p className="text-lg font-semibold">
                  Ngân sách: {project.budgetAmount.toLocaleString("vi-VN")} VNĐ
                </p>
              </div>
            )}
          </Card>

          {/* Proposals Section (Owner only) */}
          {isOwner && (
            <Card className="p-6">
              <h2 className="text-2xl font-bold mb-4">Đề xuất ({proposals.length})</h2>
              {loadingProposals ? (
                <Spinner />
              ) : proposals.length === 0 ? (
                <p className="text-gray-500">Chưa có đề xuất nào</p>
              ) : (
                <div className="space-y-4">
                  {proposals.map((proposal) => (
                    <div
                      key={proposal.id}
                      className="border rounded-lg p-4 hover:bg-gray-50 transition"
                    >
                      <div className="flex items-start justify-between">
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-2">
                            <Link
                              to={`/profile/${proposal.freelancerId}`}
                              className="font-semibold text-blue-600 hover:underline"
                            >
                              Xem profile freelancer
                            </Link>
                            <Badge variant="outline">{proposal.status}</Badge>
                          </div>
                          {proposal.coverLetter && (
                            <p className="text-gray-700 mb-2">{proposal.coverLetter}</p>
                          )}
                          {proposal.bidAmount && (
                            <p className="text-lg font-semibold text-green-600">
                              {proposal.bidAmount.toLocaleString("vi-VN")} VNĐ
                            </p>
                          )}
                          <p className="text-sm text-gray-500">
                            {new Date(proposal.createdAt).toLocaleString("vi-VN")}
                          </p>
                        </div>
                        {proposal.status === "Pending" && (
                          <Button
                            onClick={() => handleAcceptProposal(proposal.id)}
                            className="ml-4"
                          >
                            Chấp nhận
                          </Button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </Card>
          )}

          {/* Proposal Form (Non-owner only) */}
          {!isOwner && currentUserId && project.status === "Open" && (
            <Card className="p-6">
              {!showProposalForm ? (
                <Button onClick={() => setShowProposalForm(true)} className="w-full">
                  Gửi đề xuất
                </Button>
              ) : (
                <form onSubmit={handleSubmitProposal} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium mb-2">
                      Thư giới thiệu
                    </label>
                    <Textarea
                      value={proposalData.coverLetter}
                      onChange={(e) =>
                        setProposalData({ ...proposalData, coverLetter: e.target.value })
                      }
                      placeholder="Giới thiệu về bản thân và lý do bạn phù hợp với dự án này..."
                      rows={5}
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-2">
                      Giá đề xuất (VNĐ)
                    </label>
                    <Input
                      type="number"
                      value={proposalData.bidAmount}
                      onChange={(e) =>
                        setProposalData({ ...proposalData, bidAmount: e.target.value })
                      }
                      placeholder="Nhập giá đề xuất"
                      required
                      min="0"
                    />
                  </div>
                  <div className="flex gap-2">
                    <Button
                      type="submit"
                      disabled={submittingProposal}
                      className="flex-1"
                    >
                      {submittingProposal ? "Đang gửi..." : "Gửi đề xuất"}
                    </Button>
                    <Button
                      type="button"
                      variant="outline"
                      onClick={() => {
                        setShowProposalForm(false);
                        setProposalData({ coverLetter: "", bidAmount: "" });
                      }}
                    >
                      Hủy
                    </Button>
                  </div>
                </form>
              )}
            </Card>
          )}

          {!currentUserId && project.status === "Open" && (
            <Card className="p-6">
              <p className="text-center mb-4">Đăng nhập để gửi đề xuất</p>
              <div className="flex gap-2 justify-center">
                <Button onClick={() => navigate("/login")}>Đăng nhập</Button>
                <Button variant="outline" onClick={() => navigate("/register")}>
                  Đăng ký
                </Button>
              </div>
            </Card>
          )}
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          <Card className="p-6">
            <h3 className="font-semibold mb-4">Thông tin dự án</h3>
            <div className="space-y-2 text-sm">
              <div>
                <span className="text-gray-600">Trạng thái:</span>
                <Badge className="ml-2">{project.status}</Badge>
              </div>
              {project.categoryId && (
                <div>
                  <span className="text-gray-600">Danh mục:</span>
                  <span className="ml-2">{project.categoryId}</span>
                </div>
              )}
              <div>
                <span className="text-gray-600">Ngày tạo:</span>
                <span className="ml-2">
                  {new Date(project.createdAt).toLocaleDateString("vi-VN")}
                </span>
              </div>
              {project.updatedAt && (
                <div>
                  <span className="text-gray-600">Cập nhật:</span>
                  <span className="ml-2">
                    {new Date(project.updatedAt).toLocaleDateString("vi-VN")}
                  </span>
                </div>
              )}
            </div>
          </Card>

          {owner && (
            <Card className="p-6">
              <h3 className="font-semibold mb-4">Chủ dự án</h3>
              <Link
                to={`/profile/${owner.id}`}
                className="block text-blue-600 hover:underline font-semibold"
              >
                {owner.fullName}
              </Link>
              {owner.email && (
                <p className="text-sm text-gray-600 mt-1">{owner.email}</p>
              )}
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}




