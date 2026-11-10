* star comment stays
report ztest.
data lv_s type string value 'Keep CASE. inside. string'.
lv_s = 'don''t split. here'.
if lv_s cs 'x'. " trailing. comment
concatenate 'a' 'b' into lv_s separated by space.
endif.
