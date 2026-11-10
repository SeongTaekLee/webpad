SELECT matnr, -- material
  werks, -- plant
  lgort /* storage */, menge
FROM mard
WHERE werks = '1000'
