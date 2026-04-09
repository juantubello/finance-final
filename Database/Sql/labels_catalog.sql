PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS labels (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    CONSTRAINT ck_labels_name_not_blank CHECK (length(trim(name)) > 0)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_labels_name_nocase
    ON labels (name COLLATE NOCASE);

CREATE TABLE IF NOT EXISTS gasto_labels (
    gasto_id INTEGER NOT NULL,
    label_id INTEGER NOT NULL,
    PRIMARY KEY (gasto_id, label_id),
    CONSTRAINT fk_gasto_labels_gasto
        FOREIGN KEY (gasto_id) REFERENCES gastos (id) ON DELETE CASCADE,
    CONSTRAINT fk_gasto_labels_label
        FOREIGN KEY (label_id) REFERENCES labels (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_gasto_labels_label_id
    ON gasto_labels (label_id);
