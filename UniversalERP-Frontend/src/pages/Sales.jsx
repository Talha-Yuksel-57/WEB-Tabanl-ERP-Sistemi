import { useEffect, useState } from "react";
import { getSales, createSale, cancelSale } from "../api/sales";
import { getProducts } from "../api/products";
import { getCustomers } from "../api/customers";
import { hasRole, CAN_CREATE_SALE, CAN_CANCEL_SALE } from "../utils/permissions";

const emptyForm = { productId: "", customerId: "", quantity: 1, paymentMethod: "Nakit" };

function formatCurrency(n) {
  return `${Number(n || 0).toLocaleString("tr-TR", { minimumFractionDigits: 2 })} TL`;
}

export default function SalesPage({ role }) {
  const [sales, setSales] = useState([]);
  const [products, setProducts] = useState([]);
  const [customers, setCustomers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState(null);

  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);

  const canCreate = hasRole(role, CAN_CREATE_SALE);
  const canCancel = hasRole(role, CAN_CANCEL_SALE);

  async function loadAll() {
    setLoading(true);
    setErr(null);
    try {
      const [salesData, productsData, customersData] = await Promise.all([
        getSales(),
        getProducts(),
        getCustomers(),
      ]);
      setSales(salesData || []);
      setProducts(productsData || []);
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

  const selectedProduct = products.find((p) => p.id === Number(form.productId));

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    setErr(null);
    try {
      await createSale({
        productId: Number(form.productId),
        customerId: Number(form.customerId),
        quantity: Number(form.quantity),
        paymentMethod: form.paymentMethod,
      });
      setShowForm(false);
      setForm(emptyForm);
      await loadAll();
    } catch (ex) {
      // Backend stok yetersizse "Yetersiz stok. Mevcut: X, İstenen: Y" mesajı döner
      setErr(ex.message || "Satış oluşturulamadı");
    } finally {
      setSaving(false);
    }
  }

  async function handleCancel(id) {
    if (!confirm("Bu satışı iptal etmek istediğinize emin misiniz? Stok iade edilecek.")) return;
    setErr(null);
    try {
      await cancelSale(id);
      await loadAll();
    } catch (ex) {
      setErr(ex.message || "İptal başarısız");
    }
  }

  return (
    <div>
      <div className="row" style={{ justifyContent: "space-between", marginBottom: 14 }}>
        <h2 style={{ margin: 0 }}>Satışlar</h2>
        {canCreate && (
          <button className="btn" onClick={() => setShowForm(true)}>
            + Yeni Satış
          </button>
        )}
      </div>

      {err && <div className="error" style={{ marginBottom: 14 }}>{err}</div>}

      {showForm && (
        <div className="card" style={{ marginBottom: 16 }}>
          <h3 style={{ marginTop: 0 }}>Yeni Satış</h3>
          <form onSubmit={handleSubmit}>
            <div className="row" style={{ marginBottom: 12 }}>
              <div className="field">
                <label>Ürün</label>
                <select
                  required
                  value={form.productId}
                  onChange={(e) => setForm({ ...form, productId: e.target.value })}
                >
                  <option value="">Seçiniz</option>
                  {products.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name} — Stok: {p.stockCount} — {formatCurrency(p.price)}
                    </option>
                  ))}
                </select>
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
                <label>Miktar</label>
                <input
                  required
                  type="number"
                  min="1"
                  value={form.quantity}
                  onChange={(e) => setForm({ ...form, quantity: e.target.value })}
                />
              </div>
              <div className="field">
                <label>Ödeme Yöntemi</label>
                <select
                  value={form.paymentMethod}
                  onChange={(e) => setForm({ ...form, paymentMethod: e.target.value })}
                >
                  <option value="Nakit">Nakit</option>
                  <option value="Kredi Kartı">Kredi Kartı</option>
                  <option value="Havale">Havale</option>
                </select>
              </div>
            </div>

            {selectedProduct && form.quantity > 0 && (
              <div className="small" style={{ marginBottom: 12 }}>
                Toplam tutar:{" "}
                <strong>{formatCurrency(selectedProduct.price * Number(form.quantity || 0))}</strong>
                {" · "}Mevcut stok: {selectedProduct.stockCount}
              </div>
            )}

            <div className="row">
              <button className="btn" disabled={saving}>
                {saving ? "Kaydediliyor..." : "Satışı Tamamla"}
              </button>
              <button
                type="button"
                className="btn secondary"
                onClick={() => {
                  setShowForm(false);
                  setErr(null);
                }}
              >
                Vazgeç
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="card">
        {loading && <div>Yükleniyor...</div>}
        {!loading && sales.length === 0 && (
          <div style={{ color: "var(--muted)" }}>Henüz satış kaydı yok.</div>
        )}
        {!loading && sales.length > 0 && (
          <table className="table">
            <thead>
              <tr>
                <th>Müşteri</th>
                <th>Ürün</th>
                <th>Adet</th>
                <th>Tutar</th>
                <th>Ödeme</th>
                <th>Durum</th>
                <th>Tarih</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {sales.map((s) => (
                <tr key={s.id}>
                  <td>{s.customerName}</td>
                  <td>{s.productName}</td>
                  <td>{s.quantity}</td>
                  <td>{formatCurrency(s.totalAmount)}</td>
                  <td>{s.paymentMethod}</td>
                  <td>
                    <span
                      style={{
                        color: s.status === "Completed" ? "#16a34a" : "#dc2626",
                        fontWeight: 600,
                      }}
                    >
                      {s.status === "Completed" ? "Tamamlandı" : "İptal"}
                    </span>
                  </td>
                  <td style={{ color: "var(--muted)" }}>
                    {new Date(s.createdAt).toLocaleString("tr-TR")}
                  </td>
                  <td>
                    {canCancel && s.status === "Completed" && (
                      <button className="btn secondary" onClick={() => handleCancel(s.id)}>
                        İptal Et
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
