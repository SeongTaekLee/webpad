class lcl_demo definition.
public section.
methods: run importing iv_x type i
returning value(rv_y) type i.
private section.
data mv_cnt type i.
endclass.
class lcl_demo implementation.
method run.
do 3 times.
mv_cnt = mv_cnt + 1.
while mv_cnt < 10.
add 1 to mv_cnt.
endwhile.
enddo.
select * from mara into @data(ls_mara) where matnr = 'X'.
rv_y = ls_mara-brgew.
endselect.
rv_y = iv_x * 2.
endmethod.
endclass.
