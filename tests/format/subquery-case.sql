select t.id, case when t.qty > 10 then 'big' when t.qty > 5 then 'mid' else 'small' end as size,
(select max(p.price) from prices p where p.id = t.id) as max_price
from (select id, sum(qty) qty from items where del = 'X' group by id) t
left outer join master m on m.id = t.id
where t.qty between 1 and 100 or exists (select 1 from flags f where f.id = t.id)
union all
select 0, 'none', null from dual
