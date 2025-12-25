import { useEffect, useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import api from "../lib/axios";
import { jwtDecode } from "jwt-decode";
import Button from "../components/ui/button";
import Spinner from "../components/Spinner";
import { Card } from "../components/ui/card";
import Badge from "../components/ui/badge";
import { toast } from "sonner";

export default function Proposals() {
  const navigate = useNavigate();
  const [proposals, setProposals] = useState([]); // Job applications
  const [loading, setLoading] = useState(true);
  const [currentUserId, setCurrentUserId] = useState(null);
  const [filter, setFilter] = useState("all"); // all, pending, accepted, rejected, cancelled

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
      loadProposals();
    }
  }, [currentUserId, filter]);

  const loadProposals = async () => {
    try {
      setLoading(true);
      let data = await api.get(`/applications/candidate/${currentUserId}`);
      data = data?.data || data || [];
      data = data || [];
      
      // Filter proposals
      if (filter !== "all") {
        data = data.filter((p) => p.status?.toLowerCase() === filter.toLowerCase());
      }
      
      setProposals(data);
    } catch (error) {
      console.error("Error loading proposals:", error);
      toast.error("Không thể tải danh sách đề xuất");
    } finally {
      setLoading(false);
    }
  };

  const handleCancelProposal = async (proposalId) => {
    if (!confirm("Bạn có chắc chắn muốn hủy ứng tuyển này?")) return;

    try {
      await api.patch(`/applications/${proposalId}/status`, { status: "Withdrawn" });
      toast.success("Đã hủy ứng tuyển");
      loadProposals();
    } catch (error) {
      console.error("Error cancelling application:", error);
      toast.error(error?.message || error?.response?.data?.message || "Không thể hủy ứng tuyển");
    }
  };

  const getStatusBadgeVariant = (status) => {
    switch (status?.toLowerCase()) {
      case "pending":
        return "default";
      case "accepted":
        return "success";
      case "rejected":
        return "destructive";
      case "withdrawn":
        return "secondary";
      default:
        return "outline";
    }
  };

  const getStatusLabel = (status) => {
    switch (status?.toLowerCase()) {
      case "pending":
        return "Đang chờ";
      case "accepted":
        return "Đã chấp nhận";
      case "rejected":
        return "Đã từ chối";
      case "withdrawn":
        return "Đã rút";
      default:
        return status;
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
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">Đề xuất của tôi</h1>
        <p className="text-gray-600">Quản lý tất cả các đề xuất bạn đã gửi</p>
      </div>

      {/* Filters */}
      <div className="mb-6 flex gap-2 flex-wrap">
        {["all", "pending", "accepted", "rejected", "cancelled"].map((f) => (
          <Button
            key={f}
            variant={filter === f ? "default" : "outline"}
            onClick={() => setFilter(f)}
            size="sm"
          >
            {f === "all"
              ? "Tất cả"
              : f === "pending"
              ? "Đang chờ"
              : f === "accepted"
              ? "Đã chấp nhận"
              : f === "rejected"
              ? "Đã từ chối"
              : "Đã hủy"}
          </Button>
        ))}
      </div>

      {/* Proposals List */}
      {proposals.length === 0 ? (
        <Card className="p-12 text-center">
          <p className="text-gray-500 mb-4">Bạn chưa có ứng tuyển nào</p>
          <Button onClick={() => navigate("/projects")}>Xem job</Button>
        </Card>
      ) : (
        <div className="space-y-4">
          {proposals.map((proposal) => (
            <Card key={proposal.id} className="p-6">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-2">
                    <Link
                      to={`/projects/${proposal.jobId}`}
                      className="text-xl font-semibold text-blue-600 hover:underline"
                    >
                      Xem job
                    </Link>
                    <Badge variant={getStatusBadgeVariant(proposal.status)}>
                      {getStatusLabel(proposal.status)}
                    </Badge>
                  </div>

                  {proposal.coverLetter && (
                    <p className="text-gray-700 mb-3 line-clamp-2">
                      {proposal.coverLetter}
                    </p>
                  )}

                  <div className="flex items-center gap-4 text-sm text-gray-600">
                    <span>
                      {new Date(proposal.createdAt).toLocaleString("vi-VN")}
                    </span>
                  </div>
                </div>

                <div className="flex gap-2 ml-4">
                  {proposal.status === "Pending" && (
                    <>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleCancelProposal(proposal.id)}
                      >
                        Rút ứng tuyển
                      </Button>
                    </>
                  )}
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}




