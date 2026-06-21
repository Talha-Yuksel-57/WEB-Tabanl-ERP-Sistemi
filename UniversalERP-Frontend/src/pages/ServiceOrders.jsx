import { useEffect, useState } from "react";
import {
  getServiceOrders,
  createServiceOrder,
  updateServiceOrderStatus,
  deleteServiceOrder,
} from "../api/serviceOrders";
import { getCustomers } from "../api/customers";
import {
  hasRole,
  CAN_CREATE_SERVICE_ORDER,
  CAN_UPDATE_SERVICE_STATUS,
  CAN_DELETE_SERVICE_ORDER,
} from "../utils/permissions";

const STATUSES = ["Beklemede", "Tamirde", "Tamamlandı", "Teslim Edildi", "İptal"];

const emptyForm = {
  deviceName: "",
  customerId: "",
  issueDescription: "",
  serviceFee: "",
  partCost: "",
};

function formatCurrency(n) {
  return `${Number(n || 0).toLocaleString("tr-TR", { minimumFractionDigits: 2 })} TL`;
}

function statusColor(status) {
  switch (status) {
    case "Beklemede":
      return "#d97706";
    case "Tamirde":
      return "#2563eb";
    case "Tamamlandı":
      return "#16a34a";
    case "Teslim Edildi":
      return "#16a34a";
    case "İptal":
      return "#dc2626";
    default:
      return "inherit";
  }
}

export default function ServiceOrdersPage({ role }) {
  const [orders, setOrders] = useState([]);
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState(null);

  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);

  const canCreate = hasRole(role, CAN_CREATE_SERVICE_ORDER);
  const canUpdateStatus = hasRole(role, CAN_UPDATE_SERVICE_STATUS);
  const canDelete = hasRole(role, CAN_DELETE_SERVICE_ORDER);

  async function loadAll() {
    setLoading(true);
    setErr(null);
    try {
      const [ordersData, customersData] = await Promise.all([
        getServiceOrders(),
        getCustomers(),
      ]);
      setOrders(ordersData || []);
      setCustomers(customersData || []);
    } catch (ex) {
      setErr(ex.message || "Veriler yüklenemedi");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadAll();
  }, []);

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    setErr(null);
    try {
      const customer = customers.find((c) => c.id === Number(form.customerId));
      await createServiceOrder({
        deviceName: form.deviceName,
        customerId: Number(form.customerId),
        customerName: customer?.fullName || "",
        issueDescription: form.issueDescription,
        serviceFee: Number(form.serviceFee || 0),
        partCost: Number(form.partCost || 0),
      });
      setShowForm(false);
      setForm(emptyForm);
      await loadAll();
    } catch (ex) {
      setErr(ex.message || "Servis kaydı oluşturulamadı");
    } finally {
      setSaving(false);
    }
  }

  async function handleStatusChange(id, status) {
    setErr(null);
    try {
      await updateServiceOrderStatus(id, status);
      await loadAll();
    } catch (ex) {
      setErr(ex.message || "Durum güncellenemedi");
    }
  }

  async function handleDelete(id) {
    if (!confirm("Bu servis kaydını silmek istediğinize emin misiniz?")) return;
    setErr(null);
    try {
      await deleteServiceOrder(id);
      await loadAll();
    } catch (ex) {
      setErr(ex.message || "Silme başarısız");
    }
  }

  return (
    <div>
      <div className="row" style={{ justifyContent: "space-between", marginBottom: 14 }}>
        <h2 style={{ margin: 0 }}>Teknik Servis</h2>
        {canCreate && (
          <button className="btn" onClick={() => setShowForm(true)}>
            + Yeni Servis Kaydı
          </button>
        )}
      </div>

      {err && <div className="error" style={{ marginBottom: 14 }}>{err}</div>}

      {showForm && (
        <div className="card" style={{ marginBottom: 16 }}>
          <h3 style={{ marginTop: 0 }}>Yeni Servis Kaydı</h3>
          <form onSubmit={handleSubmit}>
            <div className="row" style={{ marginBottom: 12 }}>
              <div className="field">
                <label>Cihaz Adı</label>
                <input
                  required
                  value={form.deviceName}
                  onChange={(e) => setForm({ ...form, deviceName: e.target.value })}
                />
              </div>
              <div className="field">
                <label>Müşteri</label>
                <select
                  required
                  value={form.customerId}
                  onChange={(e) => setForm({ ...form, customerId: e.target.value })}
                >
                  <option value="">Seçiniz</option>
                  {customers.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.fullName}
                    </option>
                  ))}
                </select>
              </div>
              <div className="field">
                <label>Servis Ücreti</label>
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  value={form.serviceFee}
                  onChange={(e) => setForm({ ...form, serviceFee: e.target.value })}
                />
              </div>
              <div className="field">
                <label>Parça Maliyeti</label>
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  value={form.partCost}
                  onChange={(e) => setForm({ ...form, partCost: e.target.value })}
                />
              </div>
            </div>
            <div className="field" style={{ marginBottom: 12 }}>
              <label>Arıza Açıklaması</label>
              <input
                required
                value={form.issueDescription}
                onChange={(e) => setForm({ ...form, issueDescription: e.target.value })}
                style={{ width: "100%" }}
              />
            </div>
            <div className="row">
              <button className="btn" disabled={saving}>
                {saving ? "Kaydediliyor..." : "Kaydet"}
              </button>
              <button
                type="button"
                className="btn secondary"
                onClick={() => setShowForm(false)}
              >
                Vazgeç
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="card">
        {loading && <div>Yükleniyor...</div>}
        {!loading && orders.length === 0 && (
          <div style={{ color: "var(--muted)" }}>Henüz servis kaydı yok.</div>
        )}
        {!loading && orders.length > 0 && (
          <table className="table">
            <thead>
              <tr>
                <th>Cihaz</th>
                <th>Müşteri</th>
                <th>Arıza</th>
                <th>Ücret</th>
                <th>Durum</th>
                <th>Tarih</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {orders.map((o) => (
                <tr key={o.id}>
                  <td>{o.deviceName}</td>
                  <td>{o.customerName}</td>
                  <td style={{ maxWidth: 220 }}>{o.issueDescription}</td>
                  <td>{formatCurrency(o.serviceFee)}</td>
                  <td>
                    {canUpdateStatus ? (
                      <select
                        value={o.status}
                        onChange={(e) => handleStatusChange(o.id, e.target.value)}
                        style={{ color: statusColor(o.status), fontWeight: 600 }}
                      >
                        {STATUSES.map((s) => (
                          <option key={s} value={s}>
                            {s}
                          </option>
                        ))}
                      </select>
                    ) : (
                      <span style={{ color: statusColor(o.status), fontWeight: 600 }}>
                        {o.status}
                      </span>
                    )}
                  </td>
                  <td style={{ color: "var(--muted)" }}>
                    {new Date(o.createdAt).toLocaleString("tr-TR")}
                  </td>
                  <td>
                    {canDelete && (
                      <button className="btn secondary" onClick={() => handleDelete(o.id)}>
                        Sil
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
