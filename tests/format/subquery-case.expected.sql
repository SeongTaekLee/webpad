SELECT T.ID
     , CASE WHEN T.QTY > 10 THEN 'big' WHEN T.QTY > 5 THEN 'mid' ELSE 'small' END AS SIZE
     , (SELECT MAX(P.PRICE)
          FROM PRICES P
         WHERE P.ID = T.ID) AS MAX_PRICE
  FROM (SELECT ID
             , SUM(QTY) QTY
          FROM ITEMS
         WHERE DEL = 'X'
         GROUP BY ID) T
  LEFT OUTER JOIN MASTER M
    ON M.ID = T.ID
 WHERE T.QTY BETWEEN 1 AND 100
    OR EXISTS (SELECT 1
                 FROM FLAGS F
                WHERE F.ID = T.ID)
 UNION ALL
SELECT 0
     , 'none'
     , NULL
  FROM DUAL