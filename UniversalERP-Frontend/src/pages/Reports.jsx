import { useState } from "react";
import {
  downloadSalesPdf,
  downloadSalesExcel,
  downloadStockPdf,
  downloadStockExcel,
  importProductsExcel,
  importProductsJson,
  importProductsXml,
} from "../api/reports";

export default function ReportsPage() {
  const [filter, setFilter] = useState({ startDate: "", endDate: "", status: "" });
  const [busy, setBusy] = useState(null);
  const [err, setErr] = useState(null);
  const [importResult, setImportResult] = useState(null);
  const [importFile, setImportFile] = useState(null);
  const [importType, setImportType] = useState("excel");

  async function runExport(key, fn) {
    setBusy(key);
    setErr(null);
    try {
      await fn();
    } catch (ex) {
      setErr(ex.message || "Rapor oluşturulamadı");
    } finally {
      setBusy(null);
    }
  }

  async function handleImport(e) {
    e.preventDefault();
    if (!importFile) {
      setErr("Lütfen bir dosya seçin.");
      return;
    }
    setBusy("import");
    setErr(null);
    setImportResult(null);
    try {
      let result;
      if (importType === "excel") result = await importProductsExcel(importFile);
      else if (importType === "json") result = await importProductsJson(importFile);
      else result = await importProductsXml(importFile);
      setImportResult(result);
    } catch (ex) {
      setErr(ex.message || "İçe aktarma başarısız");
    } finally {
      setBusy(null);
    }
  }

  return (
    <div>
      <h2 style={{ marginTop: 0 }}>Raporlar</h2>

      {err && <div className="error" style={{ marginBottom: 14 }}>{err}</div>}

      <div className="card" style={{ marginBottom: 16 }}>
        <h3 style={{ marginTop: 0 }}>Satış Raporu</h3>
        <div className="row" style={{ marginBottom: 12 }}>
          <div className="field">
            <label>Başlangıç Tarihi</label>
            <input
              type="date"
              value={filter.startDate}
              onChange={(e) => setFilter({ ...filter, startDate: e.target.value })}
            />
          </div>
          <div className="field">
            <label>Bitiş Tarihi</label>
            <input
              type="date"
              value={filter.endDate}
              onChange={(e) => setFilter({ ...filter, endDate: e.target.value })}
            />
          </div>
          <div className="field">
            <label>Durum</label>
            <select
              value={filter.status}
              onChange={(e) => setFilter({ ...filter, status: e.target.value })}
            >
              <option value="">Tümü</option>
              <option value="Completed">Tamamlandı</option>
              <option value="Cancelled">İptal</option>
            </select>
          </div>
        </div>
        <div className="row">
          <button
            className="btn"
            disabled={busy === "sales-pdf"}
            onClick={() => runExport("sales-pdf", () => downloadSalesPdf(filter))}
          >
            {busy === "sales-pdf" ? "İndiriliyor..." : "PDF İndir"}
          </button>
          <button
            className="btn secondary"
            disabled={busy === "sales-excel"}
            onClick={() => runExport("sales-excel", () => downloadSalesExcel(filter))}
          >
            {busy === "sales-excel" ? "İndiriliyor..." : "Excel İndir"}
          </button>
        </div>
      </div>

      <div className="card" style={{ marginBottom: 16 }}>
        <h3 style={{ marginTop: 0 }}>Stok Raporu</h3>
        <div className="row">
          <button
            className="btn"
            disabled={busy === "stock-pdf"}
            onClick={() => runExport("stock-pdf", downloadStockPdf)}
          >
            {busy === "stock-pdf" ? "İndiriliyor..." : "PDF İndir"}
          </button>
          <button
            className="btn secondary"
            disabled={busy === "stock-excel"}
            onClick={() => runExport("stock-excel", downloadStockExcel)}
          >
            {busy === "stock-excel" ? "İndiriliyor..." : "Excel İndir"}
          </button>
        </div>
      </div>

      <div className="card">
        <h3 style={{ marginTop: 0 }}>Ürün İçe Aktarma</h3>
        <p style={{ color: "var(--muted)", fontSize: 13, marginTop: -6 }}>
          Excel (.xlsx), JSON veya XML dosyasından toplu ürün ekleyebilirsiniz.
        </p>
        <form onSubmit={handleImport}>
          <div className="row" style={{ marginBottom: 12 }}>
            <div className="field">
              <label>Dosya Türü</label>
              <select value={importType} onChange={(e) => setImportType(e.target.value)}>
                <option value="excel">Excel (.xlsx)</option>
                <option value="json">JSON</option>
                <option value="xml">XML</option>
              </select>
            </div>
            <div className="field">
              <label>Dosya Seç</label>
              <input
                type="file"
                accept={
                  importType === "excel" ? ".xlsx" : importType === "json" ? ".json" : ".xml"
                }
                onChange={(e) => setImportFile(e.target.files?.[0] || null)}
              />
            </div>
          </div>
          <button className="btn" disabled={busy === "import"}>
            {busy === "import" ? "Aktarılıyor..." : "İçe Aktar"}
          </button>
        </form>

        {importResult && (
          <div className="card" style={{ marginTop: 14, background: "#f8fafc" }}>
            <div>
              Toplam satır: <strong>{importResult.totalRows}</strong> · Eklenen:{" "}
              <strong style={{ color: "#16a34a" }}>{importResult.imported}</strong> · Atlanan:{" "}
              <strong style={{ color: "#dc2626" }}>{importResult.skipped}</strong>
            </div>
            {importResult.errors && importResult.errors.length > 0 && (
              <ul style={{ marginTop: 8, paddingLeft: 18 }}>
                {importResult.errors.map((e, i) => (
                  <li key={i} style={{ color: "#dc2626", fontSize: 13 }}>
                    {e}
                  </li>
                ))}
              </ul>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
