import { useEffect, useState } from "react";
import {
  getCustomers,
  createCustomer,
  updateCustomer,
  deleteCustomer,
} from "../api/customers";
import { hasRole, CAN_DELETE_CUSTOMERS } from "../utils/permissions";

const emptyForm = { fullName: "", email: "", phone: "", address: "" };

export default function CustomersPage({ role }) {
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState(null);

  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);

  const canDelete = hasRole(role, CAN_DELETE_CUSTOMERS);

  async function loadCustomers() {
    setLoading(true);
    setErr(null);
    try {
      const data = await getCustomers();
      setCustomers(data || []);
    } catch (ex) {
      setErr(ex.message || "Müşteriler yüklenemedi");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadCustomers();
  }, []);

  function openCreateForm() {
    setEditingId(null);
    setForm(emptyForm);
    setShowForm(true);
  }

  function openEditForm(c) {
    setEditingId(c.id);
    setForm({
      fullName: c.fullName,
      email: c.email || "",
      phone: c.phone || "",
      address: c.address || "",
    });
    setShowForm(true);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    setErr(null);
    try {
      const dto = {
        fullName: form.fullName,
        email: form.email || null,
        phone: form.phone || null,
        address: form.address || null,
      };
      if (editingId) {
        await updateCustomer(editingId, dto);
      } else {
        await createCustomer(dto);
      }
      setShowForm(false);
      await loadCustomers();
    } catch (ex) {
      setErr(ex.message || "Kayıt başarısız");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("Bu müşteriyi silmek istediğinize emin misiniz?")) return;
    setErr(null);
    try {
      await deleteCustomer(id);
      await loadCustomers();
    } catch (ex) {
      setErr(ex.message || "Silme başarısız");
    }
  }

  return (
    <div>
      <div className="row" style={{ justifyContent: "space-between", marginBottom: 14 }}>
        <h2 style={{ margin: 0 }}>Müşteriler</h2>
        <button className="btn" onClick={openCreateForm}>+ Yeni Müşteri</button>
      </div>

      {err && <div className="error" style={{ marginBottom: 14 }}>{err}</div>}

      {showForm && (
        <div className="card" style={{ marginBottom: 16 }}>
          <h3 style={{ marginTop: 0 }}>{editingId ? "Müşteri Düzenle" : "Yeni Müşteri"}</h3>
          <form onSubmit={handleSubmit}>
            <div className="row" style={{ marginBottom: 12 }}>
              <div className="field">
                <label>Ad Soyad</label>
                <input
                  required
                  value={form.fullName}
                  onChange={(e) => setForm({ ...form, fullName: e.target.value })}
                />
              </div>
              <div className="field">
                <label>E-posta</label>
                <input
                  type="email"
                  value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                />
              </div>
              <div className="field">
                <label>Telefon</label>
                <input
                  value={form.phone}
                  onChange={(e) => setForm({ ...form, phone: e.target.value })}
                />
              </div>
              <div className="field">
                <label>Adres</label>
                <input
                  value={form.address}
                  onChange={(e) => setForm({ ...form, address: e.target.value })}
                />
              </div>
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
        {!loading && customers.length === 0 && (
          <div style={{ color: "var(--muted)" }}>Henüz müşteri eklenmemiş.</div>
        )}
        {!loading && customers.length > 0 && (
          <table className="table">
            <thead>
              <tr>
                <th>Ad Soyad</th>
                <th>E-posta</th>
                <th>Telefon</th>
                <th>Adres</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {customers.map((c) => (
                <tr key={c.id}>
                  <td>{c.fullName}</td>
                  <td>{c.email || "-"}</td>
                  <td>{c.phone || "-"}</td>
                  <td>{c.address || "-"}</td>
                  <td>
                    <div className="row">
                      <button className="btn secondary" onClick={() => openEditForm(c)}>
                        Düzenle
                      </button>
                      {canDelete && (
                        <button className="btn secondary" onClick={() => handleDelete(c.id)}>
                          Sil
                        </button>
                      )}
                    </div>
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
