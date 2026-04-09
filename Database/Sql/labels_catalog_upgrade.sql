PRAGMA foreign_keys = ON;

WITH RECURSIVE normalized(id, current_name) AS (
    SELECT
        id,
        trim(
            replace(
                replace(
                    replace(name, char(9), ' '),
                    char(10), ' '
                ),
                char(13), ' '
            )
        ) AS current_name
    FROM labels
    WHERE name IS NOT NULL

    UNION ALL

    SELECT
        id,
        replace(current_name, '  ', ' ')
    FROM normalized
    WHERE instr(current_name, '  ') > 0
),
final_names AS (
    SELECT id, lower(current_name) AS normalized_name
    FROM normalized
    WHERE instr(current_name, '  ') = 0
)
UPDATE labels
SET name = (
    SELECT normalized_name
    FROM final_names
    WHERE final_names.id = labels.id
)
WHERE id IN (SELECT id FROM final_names);

CREATE UNIQUE INDEX IF NOT EXISTS ux_labels_name_nocase
    ON labels (name COLLATE NOCASE);
