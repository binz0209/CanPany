import { useEffect, useState } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import apiService from "../lib/api.service";
import { jwtDecode } from "jwt-decode";
import Button from "../components/ui/button";
import Spinner from "../components/Spinner";
import { Card } from "../components/ui/card";
import Badge from "../components/ui/badge";
import { toast } from "sonner";

export default function ContractDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [contract, setContract] = useState(null);
  const [project, setProject] = useState(null);
  const [client, setClient] = useState(null);
  const [freelancer, setFreelancer] = useState(null);
  const [loading, setLoading] = useState(true);
  const [currentUserId, setCurrentUserId] = useState(null);
  const [isClient, setIsClient] = useState(false);
  const [isFreelancer, setIsFreelancer] = useState(false);

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
      loadContract();
    }
  }, [id]);

  useEffect(() => {
    if (contract && currentUserId) {
      setIsClient(contract.clientId === currentUserId);
      setIsFreelancer(contract.freelancerId === currentUserId);
    }
  }, [contract, currentUserId]);

  const loadContract = async () => {
    try {
      setLoading(true);
      const data = await apiService.get(`/contracts/${id}`);
      setContract(data);

      // Load related data
      if (data.projectId) {
        try {
          const projectData = await apiService.get(`/projects/${data.projectId}`);
          setProject(projectData);
        } catch (e) {
          console.error("Error loading project:", e);
        }
      }

      if (data.clientId) {
        try {
          const clientData = await apiService.get(`/users/${data.clientId}`);
          setClient(clientData);
        } catch (e) {
          console.error("Error loading client:", e);
        }
      }

      if (data.freelancerId) {
        try {
          const freelancerData = await apiService.get(`/users/${data.freelancerId}`);
          setFreelancer(freelancerData);
        } catch (e) {
          console.error("Error loading freelancer:", e);
        }
      }
    } catch (error) {
      console.error("Error loading contract:", error);
      toast.error("Không thể tải thông tin hợp đồng");
      navigate("/account/my-projects");
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateStatus = async (newStatus) => {
    if (!confirm(`Bạn có chắc chắn muốn đổi trạng thái thành "${newStatus}"?`))
      return;

    try {
      await apiService.put(`/contracts/${id}`, {
        ...contract,
        status: newStatus,
      });
      toast.success("Đã cập nhật trạng thái hợp đồng");
      loadContract();
    } catch (error) {
      console.error("Error updating contract:", error);
      toast.error(error.response?.data?.message || "Không thể cập nhật hợp đồng");
    }
  };

  if (loading) {
    return (
      <div className="container-ld py-24">
        <Spinner />
      </div>
    );
  }

  if (!contract) {
    return (
      <div className="container-ld py-24">
        <div className="text-center">
          <h2 className="text-2xl font-bold mb-4">Không tìm thấy hợp đồng</h2>
          <Button onClick={() => navigate("/account/my-projects")}>
            Quay lại
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="container-ld py-8">
      <div className="mb-6">
        <Button variant="outline" onClick={() => navigate("/account/my-projects")}>
          ← Quay lại
        </Button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Main Content */}
        <div className="lg:col-span-2 space-y-6">
          <Card className="p-6">
            <div className="flex items-start justify-between mb-4">
              <h1 className="text-3xl font-bold">Hợp đồng #{contract.id.slice(-8)}</h1>
              <Badge
                variant={
                  contract.status === "Active"
                    ? "default"
                    : contract.status === "Completed"
                    ? "success"
                    : "secondary"
                }
              >
                {contract.status}
              </Badge>
            </div>

            {project && (
              <div className="mb-6">
                <h2 className="text-xl font-semibold mb-2">Dự án</h2>
                <Link
                  to={`/projects/${project.id}`}
                  className="text-blue-600 hover:underline font-semibold"
                >
                  {project.title}
                </Link>
                {project.description && (
                  <p className="text-gray-600 mt-2 line-clamp-2">{project.description}</p>
                )}
              </div>
            )}

            <div className="border-t pt-4 space-y-4">
              <div>
                <span className="text-gray-600">Giá thỏa thuận:</span>
                <span className="ml-2 text-xl font-semibold text-green-600">
                  {contract.agreedAmount?.toLocaleString("vi-VN")} VNĐ
                </span>
              </div>
              <div>
                <span className="text-gray-600">Ngày tạo:</span>
                <span className="ml-2">
                  {new Date(contract.createdAt).toLocaleString("vi-VN")}
                </span>
              </div>
              {contract.updatedAt && (
                <div>
                  <span className="text-gray-600">Cập nhật:</span>
                  <span className="ml-2">
                    {new Date(contract.updatedAt).toLocaleString("vi-VN")}
                  </span>
                </div>
              )}
            </div>

            {/* Status Actions */}
            {(isClient || isFreelancer) && contract.status === "Active" && (
              <div className="border-t pt-4 mt-4">
                <h3 className="font-semibold mb-2">Thao tác</h3>
                <div className="flex gap-2">
                  <Button
                    variant="outline"
                    onClick={() => handleUpdateStatus("Completed")}
                  >
                    Đánh dấu hoàn thành
                  </Button>
                  <Button
                    variant="outline"
                    onClick={() => handleUpdateStatus("Cancelled")}
                  >
                    Hủy hợp đồng
                  </Button>
                </div>
              </div>
            )}
          </Card>

          {/* Messages Link */}
          {contract.clientId && contract.freelancerId && (
            <Card className="p-6">
              <h3 className="font-semibold mb-4">Tin nhắn</h3>
              <Button
                onClick={() => {
                  const u1 =
                    contract.clientId < contract.freelancerId
                      ? contract.clientId
                      : contract.freelancerId;
                  const u2 =
                    u1 === contract.clientId
                      ? contract.freelancerId
                      : contract.clientId;
                  const convKey = `${contract.projectId}:${u1}:${u2}`;
                  navigate(`/account/messages?conversation=${encodeURIComponent(convKey)}`);
                }}
              >
                Mở tin nhắn
              </Button>
            </Card>
          )}
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {client && (
            <Card className="p-6">
              <h3 className="font-semibold mb-4">Khách hàng</h3>
              <Link
                to={`/profile/${client.id}`}
                className="block text-blue-600 hover:underline font-semibold"
              >
                {client.fullName}
              </Link>
              {client.email && (
                <p className="text-sm text-gray-600 mt-1">{client.email}</p>
              )}
            </Card>
          )}

          {freelancer && (
            <Card className="p-6">
              <h3 className="font-semibold mb-4">Freelancer</h3>
              <Link
                to={`/profile/${freelancer.id}`}
                className="block text-blue-600 hover:underline font-semibold"
              >
                {freelancer.fullName}
              </Link>
              {freelancer.email && (
                <p className="text-sm text-gray-600 mt-1">{freelancer.email}</p>
              )}
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}




