CLASS lcl_demo DEFINITION.
  PUBLIC SECTION.
    METHODS: run IMPORTING iv_x TYPE i
    RETURNING VALUE(rv_y) TYPE i.
  PRIVATE SECTION.
    DATA mv_cnt TYPE i.
ENDCLASS.
CLASS lcl_demo IMPLEMENTATION.
  METHOD run.
    DO 3 TIMES.
      mv_cnt = mv_cnt + 1.
      WHILE mv_cnt < 10.
        ADD 1 TO mv_cnt.
      ENDWHILE.
    ENDDO.
    SELECT * FROM mara INTO @DATA(ls_mara) WHERE matnr = 'X'.
      rv_y = ls_mara-brgew.
    ENDSELECT.
    rv_y = iv_x * 2.
  ENDMETHOD.
ENDCLASS.
