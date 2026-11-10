SELECT MATNR  -- material
     , WERKS  -- plant
     , LGORT /* storage */
     , MENGE
  FROM MARD
 WHERE WERKS = '1000'