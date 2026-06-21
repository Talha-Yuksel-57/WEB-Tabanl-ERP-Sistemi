import { useEffect, useState } from "react";
import {
  getProducts,
  createProduct,
  updateProduct,
  updateProductStock,
  deleteProduct,
} from "../api/products";
import { hasRole, CAN_MANAGE_PRODUCTS, CAN_DELETE_PRODUCTS, CAN_UPDATE_STOCK } from "../utils/permissions";

const emptyForm = { name: "", price: "", stockCount: "", minStockLevel: 5 };

function formatCurrency(n) {
  return `${Number(n || 0).toLocaleString("tr-TR", { minimumFractionDigits: 2 })} TL`;
}

export default function ProductsPage({ role }) {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState(null);

  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);

  const [stockEditId, setStockEditId] = useState(null);
  const [stockValue, setStockValue] = useState("");

  const canManage = hasRole(role, CAN_MANAGE_PRODUCTS);
  const canDelete = hasRole(role, CAN_DELETE_PRODUCTS);
  const canUpdateStock = hasRole(role, CAN_UPDATE_STOCK);

  async function loadProducts() {
    setLoading(true);
    setErr(null);
    try {
      const data = await getProducts();
      setProducts(data || []);
    } catch (ex) {
      setErr(ex.message || "Ürünler yüklenemedi");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadProducts();
  }, []);

  function openCreateForm() {
    setEditingId(null);
    setForm(emptyForm);
    setShowForm(true);
  }

  function openEditForm(p) {
    setEditingId(p.id);
    setForm({
      name: p.name,
      price: p.price,
      minStockLevel: p.minStockLevel,
      isActive: p.isActive,
    });
    setShowForm(true);
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    setErr(null);
    try {
      if (editingId) {
        await updateProduct(editingId, {
          name: form.name,
          price: Number(form.price),
          minStockLevel: Number(form.minStockLevel),
          isActive: form.isActive ?? true,
        });
      } else {
        await createProduct({
          name: form.name,
          price: Number(form.price),
          stockCount: Number(form.stockCount),
          minStockLevel: Number(form.minStockLevel),
        });
      }
      setShowForm(false);
      await loadProducts();
    } catch (ex) {
      setErr(ex.message || "Kayıt başarısız");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!confirm("Bu ürünü silmek istediğinize emin misiniz?")) return;
    setErr(null);
    try {
      await deleteProduct(id);
      await loadProducts();
    } catch (ex) {
      setErr(ex.message || "Silme başarısız");
    }
  }

  function openStockEdit(p) {
    setStockEditId(p.id);
    setStockValue(String(p.stockCount));
  }

  async function saveStock(id) {
    setErr(null);
    try {
      await updateProductStock(id, Number(stockValue));
      setStockEditId(null);
      await loadProducts();
    } catch (ex) {
      setErr(ex.message || "Stok güncellenemedi");
    }
  }

  return (
    <div>
      <div className="row" style={{ justifyContent: "space-between", marginBottom: 14 }}>
        <h2 style={{ margin: 0 }}>Ürünler</h2>
        {canManage && <button className="btn" onClick={openCreateForm}>+ Yeni Ürün</button>}
      </div>

      {err && <div className="error" style={{ marginBottom: 14 }}>{err}</div>}

      {showForm && (
        <div className="card" style={{ marginBottom: 16 }}>
          <h3 style={{ marginTop: 0 }}>{editingId ? "Ürün Düzenle" : "Yeni Ürün"}</h3>
          <form onSubmit={handleSubmit}>
            <div className="row" style={{ marginBottom: 12 }}>
              <div className="field">
                <label>Ürün Adı</label>
                <input
                  required
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                />
              </div>
              <div className="field">
                <label>Fiyat</label>
                <input
                  required
                  type="number"
                  step="0.01"
                  min="0.01"
                  value={form.price}
                  onChange={(e) => setForm({ ...form, price: e.target.value })}
                />
              </div>
              {!editingId && (
                <div className="field">
                  <label>Başlangıç Stoku</label>
                  <input
                    required
                    type="number"
                    min="0"
                    value={form.stockCount}
                    onChange={(e) => setForm({ ...form, stockCount: e.target.value })}
                  />
                </div>
              )}
              <div className="field">
                <label>Minimum Stok Seviyesi</label>
                <input
                  type="number"
                  min="0"
                  value={form.minStockLevel}
                  onChange={(e) => setForm({ ...form, minStockLevel: e.target.value })}
                />
              </div>
              {editingId && (
                <div className="field">
                  <label>Durum</label>
                  <select
                    value={form.isActive ? "1" : "0"}
                    onChange={(e) => setForm({ ...form, isActive: e.target.value === "1" })}
                  >
                    <option value="1">Aktif</option>
                    <option value="0">Pasif</option>
                  </select>
                </div>
              )}
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
        {!loading && products.length === 0 && (
          <div style={{ color: "var(--muted)" }}>Henüz ürün eklenmemiş.</div>
        )}
        {!loading && products.length > 0 && (
          <table className="table">
            <thead>
              <tr>
                <th>Ürün Adı</th>
                <th>Fiyat</th>
                <th>Stok</th>
                <th>Durum</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {products.map((p) => (
                <tr key={p.id}>
                  <td>{p.name}</td>
                  <td>{formatCurrency(p.price)}</td>
                  <td>
                    {stockEditId === p.id ? (
                      <div className="row">
                        <input
                          type="number"
                          min="0"
                          value={stockValue}
                          onChange={(e) => setStockValue(e.target.value)}
                          style={{ width: 90 }}
                        />
                        <button className="btn" onClick={() => saveStock(p.id)}>
                          Kaydet
                        </button>
                        <button
                          className="btn secondary"
                          onClick={() => setStockEditId(null)}
                        >
                          Vazgeç
                        </button>
                      </div>
                    ) : (
                      <span
                        style={{
                          color: p.isLowStock ? "#dc2626" : "inherit",
                          fontWeight: p.isLowStock ? 700 : 400,
                          cursor: canUpdateStock ? "pointer" : "default",
                        }}
                        title={canUpdateStock ? "Stoku düzenlemek için tıkla" : ""}
                        onClick={() => canUpdateStock && openStockEdit(p)}
                      >
                        {p.stockCount} {p.isLowStock && "⚠"}
                      </span>
                    )}
                  </td>
                  <td>
                    <span className="badge">{p.isActive ? "Aktif" : "Pasif"}</span>
                  </td>
                  <td>
                    <div className="row">
                      {canManage && (
                        <button className="btn secondary" onClick={() => openEditForm(p)}>
                          Düzenle
                        </button>
                      )}
                      {canDelete && (
                        <button className="btn secondary" onClick={() => handleDelete(p.id)}>
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
