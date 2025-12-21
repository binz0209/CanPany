import { useEffect, useState } from "react";
import apiService from "../../lib/api.service";
import Spinner from "../../components/Spinner";
import { Card } from "../../components/ui/card";
import Badge from "../../components/ui/badge";
import Input from "../../components/ui/input";
import Button from "../../components/ui/button";
import { toast } from "sonner";

export default function AuditLogs() {
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState({
    userId: "",
    endpoint: "",
    method: "",
    startDate: "",
    endDate: "",
  });
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 20;

  useEffect(() => {
    loadLogs();
  }, [currentPage, filters]);

  const loadLogs = async () => {
    try {
      setLoading(true);
      
      // Use appropriate endpoint based on filters
      let endpoint = "/auditlogs/errors";
      const params = new URLSearchParams();
      
      if (filters.userId) {
        endpoint = `/auditlogs/by-user/${filters.userId}`;
        params.append("limit", "1000");
      } else if (filters.startDate || filters.endDate) {
        endpoint = "/auditlogs/by-date-range";
        if (filters.startDate) {
          params.append("startDate", new Date(filters.startDate).toISOString());
        } else {
          const thirtyDaysAgo = new Date();
          thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
          params.append("startDate", thirtyDaysAgo.toISOString());
        }
        if (filters.endDate) {
          params.append("endDate", new Date(filters.endDate).toISOString());
        } else {
          params.append("endDate", new Date().toISOString());
        }
        params.append("limit", "1000");
      } else {
        params.append("limit", "100");
      }

      const data = await apiService.get(`${endpoint}?${params.toString()}`);
      let logs = Array.isArray(data) ? data : [];
      
      // Client-side filtering for endpoint and method
      if (filters.endpoint) {
        logs = logs.filter((log) =>
          (log.endpoint || log.path || "").toLowerCase().includes(filters.endpoint.toLowerCase())
        );
      }
      if (filters.method) {
        logs = logs.filter((log) =>
          (log.method || "").toUpperCase() === filters.method.toUpperCase()
        );
      }
      
      // Pagination
      const start = (currentPage - 1) * pageSize;
      const end = start + pageSize;
      setLogs(logs.slice(start, end));
      setTotalPages(Math.ceil(logs.length / pageSize));
    } catch (error) {
      console.error("Error loading audit logs:", error);
      toast.error("Không thể tải audit logs");
      setLogs([]);
    } finally {
      setLoading(false);
    }
  };

  const getMethodBadgeVariant = (method) => {
    switch (method?.toUpperCase()) {
      case "GET":
        return "default";
      case "POST":
        return "success";
      case "PUT":
      case "PATCH":
        return "warning";
      case "DELETE":
        return "destructive";
      default:
        return "outline";
    }
  };

  const getStatusCodeColor = (statusCode) => {
    if (statusCode >= 200 && statusCode < 300) return "text-green-600";
    if (statusCode >= 300 && statusCode < 400) return "text-blue-600";
    if (statusCode >= 400 && statusCode < 500) return "text-yellow-600";
    if (statusCode >= 500) return "text-red-600";
    return "text-gray-600";
  };

  return (
    <div className="container-ld py-8">
      <div className="mb-6">
        <h1 className="text-3xl font-bold mb-2">Audit Logs</h1>
        <p className="text-gray-600">Xem lịch sử hoạt động của hệ thống</p>
      </div>

      {/* Filters */}
      <Card className="p-6 mb-6">
        <h2 className="text-lg font-semibold mb-4">Bộ lọc</h2>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium mb-2">User ID</label>
            <Input
              value={filters.userId}
              onChange={(e) => setFilters({ ...filters, userId: e.target.value })}
              placeholder="Nhập User ID"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-2">Endpoint</label>
            <Input
              value={filters.endpoint}
              onChange={(e) => setFilters({ ...filters, endpoint: e.target.value })}
              placeholder="/api/projects"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-2">Method</label>
            <Input
              value={filters.method}
              onChange={(e) => setFilters({ ...filters, method: e.target.value })}
              placeholder="GET, POST, PUT, DELETE"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-2">Từ ngày</label>
            <Input
              type="date"
              value={filters.startDate}
              onChange={(e) => setFilters({ ...filters, startDate: e.target.value })}
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-2">Đến ngày</label>
            <Input
              type="date"
              value={filters.endDate}
              onChange={(e) => setFilters({ ...filters, endDate: e.target.value })}
            />
          </div>
          <div className="flex items-end">
            <Button
              onClick={() => {
                setFilters({
                  userId: "",
                  endpoint: "",
                  method: "",
                  startDate: "",
                  endDate: "",
                });
                setCurrentPage(1);
              }}
              variant="outline"
              className="w-full"
            >
              Xóa bộ lọc
            </Button>
          </div>
        </div>
      </Card>

      {/* Logs Table */}
      {loading ? (
        <Spinner />
      ) : logs.length === 0 ? (
        <Card className="p-12 text-center">
          <p className="text-gray-500">Không có audit log nào</p>
        </Card>
      ) : (
        <>
          <Card className="p-6">
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead>
                  <tr className="border-b">
                    <th className="text-left p-2">Thời gian</th>
                    <th className="text-left p-2">User</th>
                    <th className="text-left p-2">Method</th>
                    <th className="text-left p-2">Endpoint</th>
                    <th className="text-left p-2">Status</th>
                    <th className="text-left p-2">Duration</th>
                    <th className="text-left p-2">IP</th>
                  </tr>
                </thead>
                <tbody>
                  {logs.map((log) => (
                    <tr key={log.id} className="border-b hover:bg-gray-50">
                      <td className="p-2 text-sm">
                        {new Date(log.createdAt || log.timestamp).toLocaleString("vi-VN")}
                      </td>
                      <td className="p-2 text-sm">
                        {log.userId ? (
                          <span className="font-mono text-xs">{log.userId.slice(-8)}</span>
                        ) : (
                          <span className="text-gray-400">-</span>
                        )}
                      </td>
                      <td className="p-2">
                        <Badge variant={getMethodBadgeVariant(log.method)}>
                          {log.method}
                        </Badge>
                      </td>
                      <td className="p-2 text-sm font-mono">{log.endpoint || log.path}</td>
                      <td className="p-2">
                        <span className={getStatusCodeColor(log.statusCode)}>
                          {log.statusCode}
                        </span>
                      </td>
                      <td className="p-2 text-sm">
                        {log.duration ? `${log.duration}ms` : "-"}
                      </td>
                      <td className="p-2 text-sm font-mono text-xs">
                        {log.ipAddress || "-"}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 mt-6">
              <Button
                variant="outline"
                onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                disabled={currentPage === 1}
              >
                Trước
              </Button>
              <span className="text-sm">
                Trang {currentPage} / {totalPages}
              </span>
              <Button
                variant="outline"
                onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                disabled={currentPage === totalPages}
              >
                Sau
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  );
}

