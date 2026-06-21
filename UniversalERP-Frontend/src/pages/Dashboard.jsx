import { useEffect, useState } from "react";
import { getDashboard } from "../api/dashboard";

function Kpi({ label, value, sub, accent }) {
  return (
    <div className="card" style={{ flex: 1, minWidth: 180 }}>
      <div style={{ color: "var(--muted)", fontSize: 13 }}>{label}</div>
      <div style={{ fontSize: 26, fontWeight: 700, color: accent || "var(--text)" }}>
        {value}
      </div>
      {sub && <div style={{ color: "var(--muted)", fontSize: 12 }}>{sub}</div>}
    </div>
  );
}

function formatCurrency(n) {
  if (n == null) return "0,00 TL";
  return `${Number(n).toLocaleString("tr-TR", { minimumFractionDigits: 2 })} TL`;
}

export default function DashboardPage() {
  const [data, setData] = useState(null);
  const [err, setErr] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let mounted = true;
    setLoading(true);
    getDashboard()
      .then((res) => { if (mounted) setData(res); })
      .catch((ex) => { if (mounted) setErr(ex.message || "Dashboard verisi alınamadı"); })
      .finally(() => { if (mounted) setLoading(false); });
    return () => { mounted = false; };
  }, []);

  if (loading) return <div className="card">Yükleniyor...</div>;

  if (err)
    return (
      <div className="card error">
        Dashboard yüklenemedi: {err}
        <div style={{ color: "var(--muted)", fontSize: 12, marginTop: 6 }}>
          Backend'in çalıştığından ve token'ın geçerli olduğundan emin ol.
        </div>
      </div>
    );

  const d = data || {};

  return (
    <div>
      <h2 style={{ marginTop: 0 }}>Dashboard</h2>

      <div className="row" style={{ flexWrap: "wrap", gap: 12, marginBottom: 20 }}>
        <Kpi label="Toplam Ürün" value={d.totalProducts ?? 0} />
        <Kpi label="Toplam Müşteri" value={d.totalCustomers ?? 0} />
        <Kpi label="Bugünkü Satış" value={d.totalSalesToday ?? 0} />
        <Kpi label="Bu Ay Satış" value={d.totalSalesThisMonth ?? 0} />
        <Kpi label="Kritik Stok" value={d.lowStockProductCount ?? 0}
          accent={d.lowStockProductCount > 0 ? "#dc2626" : undefined} />
        <Kpi label="Açık Servis" value={d.openServiceOrders ?? 0} />
        <Kpi label="Bekleyen Görev" value={d.pendingTasks ?? 0} />
      </div>

      <div className="row" style={{ flexWrap: "wrap", gap: 12, marginBottom: 20 }}>
        <Kpi label="Bugünkü Gelir" value={formatCurrency(d.todayRevenue)} accent="#16a34a" />
        <Kpi label="Bu Ay Gelir" value={formatCurrency(d.thisMonthRevenue)} accent="#16a34a" />
        <Kpi label="Geçen Ay Gelir" value={formatCurrency(d.lastMonthRevenue)} />
      </div>

      <div className="row" style={{ gap: 16, alignItems: "flex-start", flexWrap: "wrap" }}>
        <div className="card" style={{ flex: 2, minWidth: 320 }}>
          <h3 style={{ marginTop: 0 }}>Son Satışlar</h3>
          {(!d.recentSales || d.recentSales.length === 0) && (
            <div style={{ color: "var(--muted)" }}>Henüz satış kaydı yok.</div>
          )}
          {d.recentSales && d.recentSales.length > 0 && (
            <table className="table">
              <thead>
                <tr><th>Müşteri</th><th>Ürün</th><th>Tutar</th><th>Durum</th><th>Tarih</th></tr>
              </thead>
              <tbody>
                {d.recentSales.map((s) => (
                  <tr key={s.id}>
                    <td>{s.customerName}</td>
                    <td>{s.productName}</td>
                    <td>{formatCurrency(s.totalAmount)}</td>
                    <td>
                      <span style={{ color: s.status === "Completed" ? "#16a34a" : "#dc2626", fontWeight: 600 }}>
                        {s.status === "Completed" ? "Tamamlandı" : "İptal"}
                      </span>
                    </td>
                    <td style={{ color: "var(--muted)" }}>{new Date(s.createdAt).toLocaleString("tr-TR")}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        <div className="card" style={{ flex: 1, minWidth: 260 }}>
          <h3 style={{ marginTop: 0 }}>Kritik Stoktaki Ürünler</h3>
          {(!d.lowStockProducts || d.lowStockProducts.length === 0) && (
            <div style={{ color: "var(--muted)" }}>Kritik stokta ürün yok.</div>
          )}
          {d.lowStockProducts && d.lowStockProducts.length > 0 && (
            <ul style={{ paddingLeft: 18, margin: 0 }}>
              {d.lowStockProducts.map((p) => (
                <li key={p.id} style={{ marginBottom: 6 }}>
                  <strong>{p.name}</strong> —{" "}
                  <span style={{ color: "#dc2626" }}>{p.stockCount} / {p.minStockLevel}</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>

      {d.topProducts && d.topProducts.length > 0 && (
        <div className="card" style={{ marginTop: 16 }}>
          <h3 style={{ marginTop: 0 }}>En Çok Satılan Ürünler</h3>
          <table className="table">
            <thead>
              <tr><th>Ürün</th><th>Toplam Adet</th><th>Toplam Gelir</th></tr>
            </thead>
            <tbody>
              {d.topProducts.map((p, i) => (
                <tr key={i}>
                  <td>{p.productName}</td>
                  <td>{p.totalQuantity}</td>
                  <td>{formatCurrency(p.totalRevenue)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}