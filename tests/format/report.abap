REPORT zdemo.
data: lv_a type i,
      lv_b type string.
START-OF-SELECTION.
if lv_a > 0.
write: / 'positive'.
elseif lv_a = 0.
write: / 'zero'.
else.
write: / 'negative'. "comment
endif.
loop at lt_tab into data(ls).
case ls-kind.
when 'A'.
perform do_a using ls.
when others.
continue.
endcase.
endloop.
form do_a using p_ls type any.
select matnr werks
  from marc
  into table lt_marc
  where werks = p_ls-werks.
try.
lv_b = |{ lv_a }|.
catch cx_root into data(lx).
message lx type 'E'.
endtry.
endform.
