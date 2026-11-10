select single matnr werks into (lv_matnr, lv_werks) from marc where matnr = @lv_matnr and werks = @lv_werks.
select * from mara into table @data(lt_mara) up to 100 rows where mtart = 'FERT' order by matnr.
