select a.matnr, a.werks, b.maktx as text, count(*) cnt
from mara as a inner join makt as b on a.matnr = b.matnr and b.spras = 'E'
where a.mtart in ('FERT','HALB') and a.lvorm = ''
group by a.matnr, a.werks, b.maktx
having count(*) > 1
order by a.matnr desc
