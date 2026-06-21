import { useEffect, useState } from "react";
import {
  getProjectTasks,
  createProjectTask,
  updateProjectTask,
  deleteProjectTask,
} from "../api/projectTasks";
import {
  hasRole,
  CAN_CREATE_TASK,
  CAN_UPDATE_TASK,
  CAN_DELETE_TASK,
} from "../utils/permissions";

const STATUSES = ["Yapılacak", "Yapılıyor", "Bitti"];
const PRIORITIES = ["Low", "Medium", "High"];

const priorityLabel = { Low: "Düşük", Medium: "Orta", High: "Yüksek" };
const priorityColor = { Low: "#16a34a", Medium: "#d97706", High: "#dc2626" };

const emptyForm = {
  taskTitle: "",
  description: "",
  projectId: "",
  priority: "Medium",
};

export default function ProjectTasksPage({ role }) {
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState(null);

  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);

  const canCreate = hasRole(role, CAN_CREATE_TASK);
  const canUpdate = hasRole(role, CAN_UPDATE_TASK);
  const canDelete = hasRole(role, CAN_DELETE_TASK);

  async function loadTasks() {
    setLoading(true);
    setErr(null);
    try {
      const data = await getProjectTasks();
      setTasks(data || []);
    } catch (ex) {
      setErr(ex.message || "Görevler yüklenemedi");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadTasks();
  }, []);

  async function handleSubmit(e) {
    e.preventDefault();
    setSaving(true);
    setErr(null);
    try {
      await createProjectTask({
        taskTitle: form.taskTitle,
        description: form.description,
        projectId: Number(form.projectId),
        priority: form.priority,
      });
      setShowForm(false);
      setForm(emptyForm);
      await loadTasks();
    } catch (ex) {
      setErr(ex.message || "Görev oluşturulamadı");
    } finally {
      setSaving(false);
    }
  }

  async function handleFieldUpdate(task, patch) {
    setErr(null);
    try {
      await updateProjectTask(task.id, {
        taskTitle: task.taskTitle,
        description: task.description,
        status: task.status,
        priority: task.priority,
        hoursWorked: task.hoursWorked,
        ...patch,
      });
      await loadTasks();
    } catch (ex) {
      setErr(ex.message || "Görev güncellenemedi");
    }
  }

  async function handleDelete(id) {
    if (!confirm("Bu görevi silmek istediğinize emin misiniz?")) return;
    setErr(null);
    try {
      await deleteProjectTask(id);
      await loadTasks();
    } catch (ex) {
      setErr(ex.message || "Silme başarısız");
    }
  }

  return (
    <div>
      <div className="row" style={{ justifyContent: "space-between", marginBottom: 14 }}>
        <h2 style={{ margin: 0 }}>Görevler</h2>
        {canCreate && (
          <button className="btn" onClick={() => setShowForm(true)}>
            + Yeni Görev
          </button>
        )}
      </div>

      {err && <div className="error" style={{ marginBottom: 14 }}>{err}</div>}

      {showForm && (
        <div className="card" style={{ marginBottom: 16 }}>
          <h3 style={{ marginTop: 0 }}>Yeni Görev</h3>
          <form onSubmit={handleSubmit}>
            <div className="row" style={{ marginBottom: 12 }}>
              <div className="field">
                <label>Görev Başlığı</label>
                <input
                  required
                  value={form.taskTitle}
                  onChange={(e) => setForm({ ...form, taskTitle: e.target.value })}
                />
              </div>
              <div className="field">
                <label>Proje ID</label>
                <input
                  required
                  type="number"
                  min="1"
                  value={form.projectId}
                  onChange={(e) => setForm({ ...form, projectId: e.target.value })}
                />
              </div>
              <div className="field">
                <label>Öncelik</label>
                <select
                  value={form.priority}
                  onChange={(e) => setForm({ ...form, priority: e.target.value })}
                >
                  {PRIORITIES.map((p) => (
                    <option key={p} value={p}>
                      {priorityLabel[p]}
                    </option>
                  ))}
                </select>
              </div>
            </div>
            <div className="field" style={{ marginBottom: 12 }}>
              <label>Açıklama</label>
              <input
                required
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
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
        {!loading && tasks.length === 0 && (
          <div style={{ color: "var(--muted)" }}>Henüz görev eklenmemiş.</div>
        )}
        {!loading && tasks.length > 0 && (
          <table className="table">
            <thead>
              <tr>
                <th>Başlık</th>
                <th>Açıklama</th>
                <th>Öncelik</th>
                <th>Saat</th>
                <th>Durum</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {tasks.map((t) => (
                <tr key={t.id}>
                  <td>{t.taskTitle}</td>
                  <td style={{ maxWidth: 220 }}>{t.description}</td>
                  <td>
                    <span style={{ color: priorityColor[t.priority], fontWeight: 600 }}>
                      {priorityLabel[t.priority] || t.priority}
                    </span>
                  </td>
                  <td>{t.hoursWorked}</td>
                  <td>
                    {canUpdate ? (
                      <select
                        value={t.status}
                        onChange={(e) => handleFieldUpdate(t, { status: e.target.value })}
                      >
                        {STATUSES.map((s) => (
                          <option key={s} value={s}>
                            {s}
                          </option>
                        ))}
                      </select>
                    ) : (
                      <span>{t.status}</span>
                    )}
                  </td>
                  <td>
                    {canDelete && (
                      <button className="btn secondary" onClick={() => handleDelete(t.id)}>
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
