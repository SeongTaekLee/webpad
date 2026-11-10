REPORT zdemo.
DATA: lv_a TYPE i,
      lv_b TYPE string.
START-OF-SELECTION.
  IF lv_a > 0.
    WRITE: / 'positive'.
  ELSEIF lv_a = 0.
    WRITE: / 'zero'.
  ELSE.
    WRITE: / 'negative'. "comment
  ENDIF.
  LOOP AT lt_tab INTO DATA(ls).
    CASE ls-kind.
      WHEN 'A'.
        PERFORM do_a USING ls.
      WHEN OTHERS.
        CONTINUE.
    ENDCASE.
  ENDLOOP.
FORM do_a USING p_ls TYPE any.
  SELECT matnr werks
    FROM marc
    INTO TABLE lt_marc
    WHERE werks = p_ls-werks.
  TRY.
    lv_b = |{ lv_a }|.
  CATCH cx_root INTO DATA(lx).
    MESSAGE lx TYPE 'E'.
  ENDTRY.
ENDFORM.
