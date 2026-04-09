PRAGMA foreign_keys = ON;

DROP INDEX IF EXISTS ix_gasto_labels_label_id;
DROP TABLE IF EXISTS gasto_labels;
DROP INDEX IF EXISTS ux_labels_name_nocase;
DROP TABLE IF EXISTS labels;
