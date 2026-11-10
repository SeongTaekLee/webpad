insert into ztab (a, b, c) values (1, 'x', sysdate);
update ztab set b = 'y', c = null where a = 1 and b <> 'z';
delete from ztab where a in (select a from zold);
