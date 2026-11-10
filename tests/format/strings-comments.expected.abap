* star comment stays
REPORT ztest.
DATA lv_s TYPE string VALUE 'Keep CASE. inside. string'.
lv_s = 'don''t split. here'.
IF lv_s CS 'x'. " trailing. comment
  CONCATENATE 'a' 'b' INTO lv_s SEPARATED BY SPACE.
ENDIF.
