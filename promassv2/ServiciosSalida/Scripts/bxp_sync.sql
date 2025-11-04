CREATE TABLE bxp_sync_ordenes (
	doc_entry INT,
	log_instance INT,
	doc_num INT,
	estatus INT,
	mensaje VARCHAR(254),
	last_update datetime null
);

CREATE INDEX pk_ordenes_docentry_loginstance
ON bxp_sync_ordenes (doc_entry, log_instance);


CREATE TABLE bxp_sync_cancelaciones_ordenes (
	doc_entry INT,
	log_instance INT,
	doc_num INT,
	estatus INT,
	mensaje VARCHAR(254),
	last_update datetime null
);

CREATE TABLE bxp_sync_cierre_ordenes (
	doc_entry INT,
	log_instance INT,
	doc_num INT,
	estatus INT,
	mensaje VARCHAR(254),
	last_update datetime null
);

CREATE TABLE bxp_sync_proveedores (
	doc_entry int,
	card_code VARCHAR(15),
	log_instance INT,
	estatus INT,
	mensaje VARCHAR(254),
	last_update datetime
);


CREATE INDEX pk_proveedores_cardcode_loginstance
ON bxp_sync_proveedores (card_code, log_instance);

CREATE INDEX ix_proveedores_docentry
ON bxp_sync_proveedores(doc_entry)