PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS label_rules (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    keyword TEXT NOT NULL,
    created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ck_label_rules_keyword_not_blank CHECK (length(trim(keyword)) > 0)
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_label_rules_keyword_nocase
    ON label_rules (keyword COLLATE NOCASE);

CREATE TABLE IF NOT EXISTS label_rule_labels (
    rule_id INTEGER NOT NULL,
    label_id INTEGER NOT NULL,
    PRIMARY KEY (rule_id, label_id),
    CONSTRAINT fk_label_rule_labels_rule
        FOREIGN KEY (rule_id) REFERENCES label_rules (id) ON DELETE CASCADE,
    CONSTRAINT fk_label_rule_labels_label
        FOREIGN KEY (label_id) REFERENCES labels (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_label_rule_labels_label_id
    ON label_rule_labels (label_id);
