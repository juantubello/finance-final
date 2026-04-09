PRAGMA foreign_keys = ON;

DROP INDEX IF EXISTS ix_label_rule_labels_label_id;
DROP TABLE IF EXISTS label_rule_labels;
DROP INDEX IF EXISTS ux_label_rules_keyword_nocase;
DROP TABLE IF EXISTS label_rules;
