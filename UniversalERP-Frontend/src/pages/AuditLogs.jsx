import { useEffect, useState } from "react";
import { getAuditLogs } from "../api/auditLogs";

const ACTION_LABEL = { Create: "Oluşturma", Update: "Güncelleme", Delete: "Silme", Cancel: "İptal" };

export default function AuditLogsPage() {
  const [logs, setLogs] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState(null);

  const [filter, setFilter] = useState({ entityName: "", action: "" });
  const pageSize = 20;

  async function loadLogs(targetPage = 1) {
    setLoading(true);
    setErr(null);
    try {
      const res = await getAuditLogs({
        ...filter,
        page: targetPage,
        pageSize,
      });
      setLogs(res.data || []);
      setTotal(res.total || 0);
      setPage(targetPage);
    } catch (ex) {
      setErr(ex.message || "Kayıtlar yüklenemedi");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadLogs(1);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <div>
      <h2 style={{ marginTop: 0 }}>İşlem Geçmişi (Audit Log)</h2>

      {err && <div className="error" style={{ marginBottom: 14 }}>{err}</div>}

      <div className="card" style={{ marginBottom: 16 }}>
        <div className="row" style={{ marginBottom: 12 }}>
          <div className="field">
            <label>Varlık Adı</label>
            <input
              placeholder="Örn: Sale, Product"
              value={filter.entityName}
              onChange={(e) => setFilter({ ...filter, entityName: e.target.value })}
            />
          </div>
          <div className="field">
            <label>İşlem</label>
            <select
              value={filter.action}
              onChange={(e) => setFilter({ ...filter, action: e.target.value })}
            >
              <option value="">Tümü</option>
              <option value="Create">Oluşturma</option>
              <option value="Update">Güncelleme</option>
              <option value="Delete">Silme</option>
              <option value="Cancel">İptal</option>
            </select>
          </div>
        </div>
        <button className="btn" onClick={() => loadLogs(1)}>
          Filtrele
        </button>
      </div>

      <div className="card">
        {loading && <div>Yükleniyor...</div>}
        {!loading && logs.length === 0 && (
          <div style={{ color: "var(--muted)" }}>Kayıt bulunamadı.</div>
        )}
        {!loading && logs.length > 0 && (
          <>
            <table className="table">
              <thead>
                <tr>
                  <th>Kullanıcı</th>
                  <th>İşlem</th>
                  <th>Varlık</th>
                  <th>Kayıt ID</th>
                  <th>Tarih</th>
                </tr>
              </thead>
              <tbody>
                {logs.map((l) => (
                  <tr key={l.id}>
                    <td>{l.userEmail || "-"}</td>
                    <td>{ACTION_LABEL[l.action] || l.action}</td>
                    <td>{l.entityName}</td>
                    <td>{l.entityId}</td>
                    <td style={{ color: "var(--muted)" }}>
                      {new Date(l.timestamp).toLocaleString("tr-TR")}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>

            <div className="row" style={{ justifyContent: "space-between", marginTop: 12 }}>
              <span className="small">
                Toplam {total} kayıt · Sayfa {page}/{totalPages}
              </span>
              <div className="row">
                <button
                  className="btn secondary"
                  disabled={page <= 1}
                  onClick={() => loadLogs(page - 1)}
                >
                  Önceki
                </button>
                <button
                  className="btn secondary"
                  disabled={page >= totalPages}
                  onClick={() => loadLogs(page + 1)}
                >
                  Sonraki
                </button>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
