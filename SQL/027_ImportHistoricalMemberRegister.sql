USE UASU_Finance_Web;  -- adjust if your database name differs
GO

-- ====================================================================
-- One-time historical import of the 2024 and 2025 Member/Agency Payer
-- registers from the existing Excel workbook (UASU_2024-2025 Member
-- Register.xlsx), so the new Union_Contributors/Union_Registrations/
-- Union_Contributions tables (SQL/026) aren't empty on day one and the
-- RTU submission history for both years is preserved digitally.
-- ====================================================================

-- Phase 1: contributors (deduped by PF No. across all 4 sheets)
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3111')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3111', 'AMOI ELIUD V');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2518')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2518', 'ANDIKA MARY A');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2304')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2304', 'ASIAGO NATHAN O');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2543')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2543', 'AYIEYE OKUMU J');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2678')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2678', 'BONIFACE NGARI IRERI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2705')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2705', 'CAROLINE MONGINA MATARA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2339')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2339', 'CHEBET LUKE K');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2335')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2335', 'CHEPKIRUI TOWET LILY');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2632')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2632', 'CHIRCHIR DAVID K');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2674')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2674', 'CYNTHIA AMAI IKAMARI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3336')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3336', 'CYRUS WANJOHI MWANGI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2686')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2686', 'DR. ANTHONY NYUTU KARANJAH');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2690')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2690', 'DR. DAVID WEKESA WAFULA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2696')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2696', 'DR. ENG. ELISHA AKECH OCHUNGO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2685')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2685', 'DR. WYCLIFFE ARANI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2703')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2703', 'DR.ALEX MAGEMBE MARUCHA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2328')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2328', 'EMAASE PATRICK M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2539')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2539', 'ERIC MUNENE NJOGU');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2695')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2695', 'ERICK AKIVAGA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2332')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2332', 'GAKINYA DAVID KARIENYE');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2349')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2349', 'GATHURA T G');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2665')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2665', 'GEOFFREY KIHARA RURIMO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2536')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2536', 'GEORGE M. MOCHECHE');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2401')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2401', 'GITILE JOSEPH NAITULI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2684')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2684', 'GLADYS MORAA NYACHIO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2675')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2675', 'IDAH GATWIRI MUCHUNKU');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2554')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2554', 'IYAYA WANJALA C');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2692')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2692', 'JOB OTIENO BONYO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2673')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2673', 'JOHNSON OTIENO OKEYO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3340')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3340', 'JOHNSTON KAMUTI KALWE');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2625')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2625', 'JOSEPHINE WAMBUI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2323')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2323', 'JOTHAM KILIMO MWALE');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2540')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2540', 'KAMAU FLORENCE N');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2324')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2324', 'KAMWATI FRANCIS M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2626')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2626', 'KARANJA JOHN PATRICK');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2325')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2325', 'KARIUKI ISAACK');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2631')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2631', 'KEMBOI KIPKIRUI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2308')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2308', 'KERRE DORCAS A');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2637')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2637', 'KIGATIIRA KINYA KATHURE');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2313')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2313', 'KIIRU DISHON T');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2502')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2502', 'KIJANA EUNICE A');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2532')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2532', 'KINUTHIA AUGUSTINE M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2628')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2628', 'KINYANJUI WANJIRU A.');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2547')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2547', 'KIPYEGON KOECH EDWIN');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2348')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2348', 'KIRAGU HENRY M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2537')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2537', 'KITAVI MUNYAO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2516')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2516', 'KIVEU MARY NAFULA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2568')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2568', 'KUNG''U JOEL NGUGI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2587')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2587', 'KUNG''U RACHEL M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2515')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2515', 'LAIMARU SILAS M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2506')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2506', 'MACHINI SYLVIA M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2560')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2560', 'MAGU MARTIN MBUGUA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3255')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3255', 'MAINA GEOFFREY MANOTI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2402')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2402', 'MAK''OCHIENG MUREJ O.');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2523')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2523', 'MAKICHE JOSIAH K');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3306')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3306', 'MARGARET WAIRIMU NGUYO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2541')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2541', 'MARY MUGO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2650')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2650', 'MASAVIRU WARREN MUSINDI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2653')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2653', 'MAWEU JONATHAN KATHUNGU');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2406')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2406', 'MAYAKA ABEL N.');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2107')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2107', 'MBATIA PAUL N');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2327')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2327', 'MLECHA DONALD F');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2657')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2657', 'MTANGE MARGARET MULEKANI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2624')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2624', 'MUHATIA CATHERINE MISIKO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2346')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2346', 'MUKABI MARY W.');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2646')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2646', 'MUSIOMI TIMOTHY MUSEMBE');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2635')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2635', 'MUSYOKI ANTHONY M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2639')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2639', 'MUTEGI JOY ELOSY');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2613')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2613', 'MUTHINI FAITH MUTANU');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2701')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2701', 'MUTINDA MUTISYA KYULE');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2519')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2519', 'MUTUA JACKSON M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2501')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2501', 'MUTUNGA ISAAC M.');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2535')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2535', 'MUTURI PETER N');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3280')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3280', 'MWANGI KELVIN KARIUKI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3348')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3348', 'Mercyline KERUBO NYAMWAYA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2565')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2565', 'NAMASWA SOLOMON W');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2644')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2644', 'NDEGWA WALTER KIRIKA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2331')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2331', 'NDITHI HENRY K');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2505')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2505', 'NDUNGU BENSON M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2513')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2513', 'NG''ANG''A PETER M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2550')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2550', 'NGALA ODEGI W');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2642')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2642', 'NGIGI ANASTASIAH NJOKI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2405')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2405', 'NGOO LIVINGSTONE M.H.');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2533')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2533', 'NICHOLAS ALEX GACHUI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2621')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2621', 'NJERU ABRAHAM KIREA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2567')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2567', 'NJIRU NICHOLAS M');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2333')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2333', 'NJOROGE KARANJA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2656')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2656', 'NJUGUNA SARAH WAIRIMU');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2634')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2634', 'NTHOKI BARBARA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2521')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2521', 'NZOMA JENIFFER');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2407')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2407', 'OCHIENG FRAZIER L.');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2683')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2683', 'OCHIENG NOAH OTIENDE');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2614')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2614', 'ODHONG EDWARD');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2622')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2622', 'OGUTA SAUL MOGUSU');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2314')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2314', 'OKADAPAU MOSES ODEO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2620')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2620', 'OKUKU BERNARD O');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2616')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2616', 'OMINA JAMES ADUNYA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2522')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2522', 'ONDIEKI M CHARLES');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2633')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2633', 'ONYANGO CHRISTOPHER W');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2531')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2531', 'ORINA GLADYS K');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2529')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2529', 'OSANO FREDRICK O');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2555')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2555', 'OTIENO CHRISTOPHER');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2651')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2651', 'OTUKANA YVETTE AWOUR');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2627')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2627', 'OYUUH SAMUEL OTIENO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2629')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2629', 'PAMBO VIVIANNE L.A');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2704')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2704', 'PETER KIPROTICH YEGON');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3350')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3350', 'PETER NYAGA MACHARIA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3344')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3344', 'REGINAH WANGUI NGARI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3334')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3334', 'RICHARD N MUGERA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3337')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3337', 'ROSE K.N MAYIANDA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3351')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3351', 'SAMMY KIPLAGAT CHEBON');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2318')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2318', 'SAMUEL ODOYO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2636')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2636', 'THAIRU JOYCE WANJIRU');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2340')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2340', 'THIONG''O GEORGE K');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2706')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2706', 'TIMOTHY ONGINO OLUCHIRI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2542')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2542', 'UGANGU WILSON');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2659')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2659', 'USAGI ALFRED KIDAHA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2680')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2680', 'VALENTINE SIYOI');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '3343')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('3343', 'VERONICA MUTINDI MASILA');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2504')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2504', 'WAGUMBA COLLINS .A.');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2310')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2310', 'WALUBENGO JOHN N');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2517')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2517', 'WANJOHI CHARLES W');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2408')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2408', 'WATTANGA IRENE S');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2654')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2654', 'WAWERU JOSPHAT KARUNGO');
IF NOT EXISTS (SELECT 1 FROM Union_Contributors WHERE PF_No = '2538')
    INSERT INTO Union_Contributors (PF_No, Full_Name) VALUES ('2538', 'YEGON ERIC KIPROB');
GO

-- Phase 2: registrations (one per contributor per year - no type here,
-- since a contributor's monthly Membership_Type can differ within the
-- same year; see the 13 people who switched status mid-2025)
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2107' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2107';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2107' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2107';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2304' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2304';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2304' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2304';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2308' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2308';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2310' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2310';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2313' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2313';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2313' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2313';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2314' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2314';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2314' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2314';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2318' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2318';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2318' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2318';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2323' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2323';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2323' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2323';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2324' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2324';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2324' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2324';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2325' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2325';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2325' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2325';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2327' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2327';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2327' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2327';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2328' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2328';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2328' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2328';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2331' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2331';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2331' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2331';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2332' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2332';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2332' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2332';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2333' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2333';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2333' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2333';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2335' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2335';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2335' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2335';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2339' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2339';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2339' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2339';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2340' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2340';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2340' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2340';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2346' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2346';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2346' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2346';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2348' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2348';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2348' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2348';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2349' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2349';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2349' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2349';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2401' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2401';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2401' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2401';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2402' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2402';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2402' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2402';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2405' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2405';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2405' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2405';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2406' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2406';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2406' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2406';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2407' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2407';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2407' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2407';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2408' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2408';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2501' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2501';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2501' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2501';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2502' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2502';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2502' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2502';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2504' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2504';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2504' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2504';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2505' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2505';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2505' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2505';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2506' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2506';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2506' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2506';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2513' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2513';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2513' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2513';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2515' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2515';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2516' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2516';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2516' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2516';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2517' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2517';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2517' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2517';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2518' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2518';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2518' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2518';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2519' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2519';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2519' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2519';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2521' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2521';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2521' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2521';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2522' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2522';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2522' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2522';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2523' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2523';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2523' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2523';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2529' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2529';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2529' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2529';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2531' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2531';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2531' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2531';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2532' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2532';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2532' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2532';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2533' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2533';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2533' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2533';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2535' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2535';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2535' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2535';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2536' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2536';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2536' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2536';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2537' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2537';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2537' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2537';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2538' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2538';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2538' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2538';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2539' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2539';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2539' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2539';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2540' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2540';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2540' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2540';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2541' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2541';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2541' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2541';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2542' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2542';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2542' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2542';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2543' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2543';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2543' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2543';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2547' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2547';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2547' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2547';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2550' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2550';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2550' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2550';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2554' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2554';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2554' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2554';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2555' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2555';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2555' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2555';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2560' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2560';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2560' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2560';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2565' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2565';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2565' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2565';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2567' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2567';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2567' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2567';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2568' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2568';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2568' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2568';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2587' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2587';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2587' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2587';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2613' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2613';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2613' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2613';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2614' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2614';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2614' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2614';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2616' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2616';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2616' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2616';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2620' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2620';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2620' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2620';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2621' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2621';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2621' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2621';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2622' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2622';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2622' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2622';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2624' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2624';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2624' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2624';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2625' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2625';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2625' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2625';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2626' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2626';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2626' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2626';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2627' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2627';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2627' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2627';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2628' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2628';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2628' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2628';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2629' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2629';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2629' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2629';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2631' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2631';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2631' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2631';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2632' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2632';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2632' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2632';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2633' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2633';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2634' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2634';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2634' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2634';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2635' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2635';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2635' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2635';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2636' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2636';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2636' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2636';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2637' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2637';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2637' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2637';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2639' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2639';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2639' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2639';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2642' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2642';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2642' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2642';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2644' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2644';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2644' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2644';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2646' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2646';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2646' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2646';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2650' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2650';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2650' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2650';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2651' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2651';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2651' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2651';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2653' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2653';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2653' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2653';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2654' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2654';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2654' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2654';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2656' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2656';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2656' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2656';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2657' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2657';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2657' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2657';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2659' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2659';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2659' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2659';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2665' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2665';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2665' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2665';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2673' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2673';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2673' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2673';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2674' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2674';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2674' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2674';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2675' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2675';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2675' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2675';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2678' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2678';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2678' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2678';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2680' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2680';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2680' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2680';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2683' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2683';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2683' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2683';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2684' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2684';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2684' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2684';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2685' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2685';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2685' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2685';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2686' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2686';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2686' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2686';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2690' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2690';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2690' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2690';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2692' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2692';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2692' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2692';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2695' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2695';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2695' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2695';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2696' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2696';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2696' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2696';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2701' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '2701';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2701' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2701';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2703' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2703';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2704' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2704';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2705' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2705';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '2706' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '2706';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3111' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3111';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3255' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3255';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3255' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3255';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3280' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3280';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3280' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3280';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3306' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3306';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3306' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3306';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3334' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3334';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3334' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3334';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3336' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3336';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3337' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3337';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3337' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3337';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3340' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3340';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3340' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3340';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3343' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3343';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3343' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3343';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3344' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3344';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3344' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3344';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3348' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3348';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3348' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3348';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3350' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3350';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3350' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3350';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3351' AND r.Register_Year = 2024)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2024 FROM Union_Contributors WHERE PF_No = '3351';
IF NOT EXISTS (SELECT 1 FROM Union_Registrations r INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID WHERE c.PF_No = '3351' AND r.Register_Year = 2025)
    INSERT INTO Union_Registrations (Contributor_ID, Register_Year)
    SELECT Contributor_ID, 2025 FROM Union_Contributors WHERE PF_No = '3351';
GO

-- Phase 3: monthly contributions, each carrying its own Membership_Type
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2304' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3566.58), (2, 3566.58), (3, 3566.58), (4, 3566.58), (5, 3566.58), (6, 3566.58), (7, 3566.58), (8, 3566.58), (9, 3566.58), (10, 3566.58), (11, 3566.58), (12, 3566.58)) v(Month, Amount)
WHERE c.PF_No = '2308' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2310' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2313' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4501.86), (2, 4501.86), (3, 4501.86), (4, 4501.86), (5, 4501.86), (6, 4501.86), (7, 4501.86), (8, 4501.86), (9, 4501.86), (10, 4501.86), (11, 4501.86), (12, 4501.86)) v(Month, Amount)
WHERE c.PF_No = '2314' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2186.34), (2, 2186.34), (3, 2186.34), (4, 2186.34), (5, 2186.34), (6, 2186.34), (7, 2186.34), (8, 2186.34), (9, 2186.34), (10, 2186.34), (11, 2186.34), (12, 2186.34)) v(Month, Amount)
WHERE c.PF_No = '2318' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2323' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2324' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2325' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2327' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2328' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2331' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3116.64), (2, 3116.64), (3, 3116.64), (4, 3116.64), (5, 3116.64), (6, 3116.64), (7, 3116.64), (8, 3116.64), (9, 3116.64), (10, 3116.64), (11, 3116.64), (12, 3116.64)) v(Month, Amount)
WHERE c.PF_No = '2332' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2333' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2335' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2339' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2340' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3566.6), (2, 3566.6), (3, 3566.6), (4, 3566.6), (5, 3566.6), (6, 3566.6), (7, 3566.6), (8, 3566.6), (9, 3566.6), (10, 3566.6), (11, 3566.6), (12, 3566.6)) v(Month, Amount)
WHERE c.PF_No = '2346' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3566.6), (2, 3566.6), (3, 3566.6), (4, 3566.6), (5, 3566.6), (6, 3566.6), (7, 3566.6), (8, 3566.6), (9, 3566.6), (10, 3566.6), (11, 3566.6), (12, 3566.6)) v(Month, Amount)
WHERE c.PF_No = '2348' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2349' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (4, 5661.74), (9, 5661.74), (10, 5661.74), (11, 5661.74), (12, 5661.74)) v(Month, Amount)
WHERE c.PF_No = '2401' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 5013.36), (2, 5013.36), (3, 5013.36), (4, 5013.36), (5, 5013.36), (6, 5013.36), (7, 5013.36), (8, 5013.36), (9, 5013.36), (10, 5013.36), (11, 5013.36), (12, 5013.36)) v(Month, Amount)
WHERE c.PF_No = '2402' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 5394.86)) v(Month, Amount)
WHERE c.PF_No = '2405' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 5661.74), (2, 5661.74), (3, 5661.74), (4, 5661.74), (5, 5661.74), (6, 5661.74), (7, 5661.74), (8, 5661.74), (9, 5661.74), (10, 5661.74), (11, 5661.74), (12, 5661.74)) v(Month, Amount)
WHERE c.PF_No = '2406' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2407' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4131.26), (2, 4131.26), (3, 4131.26), (4, 4131.26), (5, 4131.26), (6, 4131.26), (7, 4131.26), (8, 4131.26), (9, 4131.26), (10, 4131.26), (11, 4131.26), (12, 4131.26)) v(Month, Amount)
WHERE c.PF_No = '2408' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4131.26), (2, 4131.26), (3, 4131.26), (4, 4131.26), (5, 4131.26), (6, 4131.26), (7, 4131.26), (8, 4131.26), (9, 4131.26), (10, 4131.26), (11, 4131.26), (12, 4131.26)) v(Month, Amount)
WHERE c.PF_No = '2501' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3566.6), (2, 3566.6), (3, 3566.6), (4, 3566.6), (5, 3566.6), (6, 3566.6), (7, 3679.54), (8, 3679.54), (9, 3679.54), (10, 3679.54), (11, 3679.54), (12, 3679.54)) v(Month, Amount)
WHERE c.PF_No = '2502' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3566.6), (2, 3566.6), (3, 3566.6), (4, 3566.6), (5, 3566.6), (6, 3566.6), (7, 3679.54), (8, 3679.54), (9, 3679.54), (10, 3679.54), (11, 3679.54), (12, 3679.54)) v(Month, Amount)
WHERE c.PF_No = '2504' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.16), (2, 3207.16), (3, 3207.16), (4, 3207.16), (5, 3207.16), (6, 3207.16), (7, 3207.16), (8, 3207.16), (9, 3207.16), (10, 3207.16), (11, 3207.16), (12, 3207.16)) v(Month, Amount)
WHERE c.PF_No = '2505' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.16), (2, 3207.16), (3, 3207.16), (4, 3207.16), (5, 3207.16), (6, 3207.16), (7, 3207.16), (8, 3207.16), (9, 3207.16), (10, 3207.16), (11, 3207.16), (12, 3207.16)) v(Month, Amount)
WHERE c.PF_No = '2506' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3026.16), (2, 3116.66), (3, 3116.66), (4, 3116.66), (5, 3116.66), (6, 3116.66), (7, 3116.66), (8, 3116.66), (9, 3116.66), (10, 3116.66), (11, 3116.66), (12, 3116.66)) v(Month, Amount)
WHERE c.PF_No = '2513' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3453.68), (2, 3566.6), (3, 3566.6), (4, 3566.6), (5, 3566.6), (6, 3566.6), (7, 3566.6), (8, 3566.6), (9, 3566.6), (10, 3566.6), (11, 3566.6), (12, 3566.6)) v(Month, Amount)
WHERE c.PF_No = '2516' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2518' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2935.68), (2, 3026.16), (3, 3026.16), (4, 3026.16), (5, 3026.16), (6, 3026.16), (7, 3026.16), (8, 3026.16), (9, 3026.16), (10, 3026.16), (11, 3026.16), (12, 3026.16)) v(Month, Amount)
WHERE c.PF_No = '2519' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2935.68), (2, 3026.16), (3, 3026.16), (4, 3026.16), (5, 3026.16), (6, 3026.16), (7, 3026.16), (8, 3026.16), (9, 3026.16), (10, 3026.16), (11, 3026.16), (12, 3026.16)) v(Month, Amount)
WHERE c.PF_No = '2521' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 5013.36), (2, 5013.36), (3, 5013.36), (4, 5013.36), (5, 5013.36), (6, 5013.36), (7, 5013.36), (8, 5013.36), (9, 5013.36), (10, 5013.36), (11, 5013.36), (12, 5013.36)) v(Month, Amount)
WHERE c.PF_No = '2522' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2529' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2531' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2532' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2533' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3116.66), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2535' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2538' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3905.42), (2, 4018.34), (3, 4018.34), (4, 4018.34), (5, 4018.34), (6, 4018.34), (7, 4018.34), (8, 4018.34), (9, 4018.34), (10, 4018.34), (11, 4018.34), (12, 4018.34)) v(Month, Amount)
WHERE c.PF_No = '2539' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4131.26), (2, 4131.26), (3, 4131.26), (4, 4131.26), (5, 4131.26), (6, 4131.26), (7, 4131.26), (8, 4131.26), (9, 4131.26), (10, 4131.26), (11, 4131.26), (12, 4131.26)) v(Month, Amount)
WHERE c.PF_No = '2540' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4373.98), (2, 4373.98), (3, 4373.98), (4, 4373.98), (5, 4373.98), (6, 4373.98), (7, 4501.86), (8, 4501.86), (9, 4501.86), (10, 4501.86), (11, 4501.86), (12, 4501.86)) v(Month, Amount)
WHERE c.PF_No = '2542' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2543' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2547' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2550' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4131.26), (2, 4131.26), (3, 4131.26), (4, 4131.26), (5, 4131.26), (6, 4131.26), (7, 4131.26), (8, 4131.26), (9, 4131.26), (10, 4131.26), (11, 4131.26), (12, 4131.26)) v(Month, Amount)
WHERE c.PF_No = '2555' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (2, 2211.82), (3, 2211.82), (4, 2211.82), (5, 2211.82), (6, 2211.82), (7, 2211.82), (8, 2211.82), (9, 2211.82), (10, 2211.82), (11, 2211.82), (12, 2211.82)) v(Month, Amount)
WHERE c.PF_No = '2565' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 1956.84), (2, 1956.84), (3, 1956.84), (4, 1956.84), (5, 2033.34), (6, 2033.34), (7, 2109.84), (8, 2109.84), (9, 2109.84), (10, 2109.84), (11, 2109.84), (12, 2109.84)) v(Month, Amount)
WHERE c.PF_No = '2567' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2587' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2613' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3453.66), (2, 3453.66), (3, 3453.66), (4, 3453.66), (5, 3453.66), (6, 3453.66), (7, 3566.6), (8, 3566.6), (9, 3566.6), (10, 3566.6), (11, 3566.6), (12, 3566.6)) v(Month, Amount)
WHERE c.PF_No = '2614' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3026.18), (2, 3026.18), (3, 3026.18), (4, 3026.18), (5, 3026.18), (6, 3026.18), (7, 3116.66), (8, 3116.66), (9, 3116.66), (10, 3116.66), (11, 3116.66), (12, 3116.66)) v(Month, Amount)
WHERE c.PF_No = '2616' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.16), (2, 3207.16), (3, 3207.16), (4, 3207.16), (5, 3207.16), (6, 3207.16), (7, 3207.16), (8, 3207.16), (9, 3207.16), (10, 3207.16), (11, 3207.16), (12, 3207.16)) v(Month, Amount)
WHERE c.PF_No = '2620' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2621' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2622' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3026.18), (2, 3026.18), (3, 3026.18), (4, 3026.18), (5, 3026.18), (6, 3026.18), (7, 3116.66), (8, 3116.66), (9, 3116.66), (10, 3116.66), (11, 3116.66), (12, 3116.66)) v(Month, Amount)
WHERE c.PF_No = '2624' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3026.18), (2, 3026.18), (3, 3026.18), (4, 3026.18), (5, 3026.18), (6, 3026.18), (7, 3116.66), (8, 3116.66), (9, 3116.66), (10, 3116.66), (11, 3116.66), (12, 3116.66)) v(Month, Amount)
WHERE c.PF_No = '2625' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2626' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2627' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2628' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2629' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2631' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3026.18), (2, 3026.18), (3, 3026.18), (4, 3026.18), (5, 3026.18), (6, 3026.18), (7, 3116.66), (8, 3116.66), (9, 3116.66), (10, 3116.66), (11, 3116.66), (12, 3116.66)) v(Month, Amount)
WHERE c.PF_No = '2632' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2633' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3026.18), (2, 3026.18), (3, 3026.18), (4, 3026.18), (5, 3026.18), (6, 3026.18), (7, 3116.66), (8, 3116.66), (9, 3116.66), (10, 3116.66), (11, 3116.66), (12, 3116.66)) v(Month, Amount)
WHERE c.PF_No = '2634' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3), (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2635' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3453.66), (2, 3453.66), (3, 3453.66), (4, 3453.66), (5, 3453.66), (6, 3453.66), (7, 3566.6), (8, 3566.6), (9, 3566.6), (10, 3566.6), (11, 3566.6), (12, 3566.6)) v(Month, Amount)
WHERE c.PF_No = '2637' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2639' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 5013.36), (2, 5013.36), (3, 5013.36), (4, 5013.36), (5, 5013.36), (6, 5013.36), (7, 5013.36), (8, 5013.36), (9, 5013.36), (10, 5013.36), (11, 5013.36), (12, 5013.36)) v(Month, Amount)
WHERE c.PF_No = '2642' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2644' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2646' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3026.18), (2, 3026.18), (3, 3026.18), (4, 3026.18), (5, 3026.18), (6, 3026.18), (7, 3116.66), (8, 3116.66), (9, 3116.66), (10, 3116.66), (11, 3116.66), (12, 3116.66)) v(Month, Amount)
WHERE c.PF_No = '2650' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3026.18), (2, 3026.18), (3, 3026.18), (4, 3026.18), (5, 3026.18), (6, 3026.18), (7, 3116.66), (8, 3116.66), (9, 3116.66), (10, 3116.66), (11, 3116.66), (12, 3116.66)) v(Month, Amount)
WHERE c.PF_No = '2651' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2653' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3116.64), (2, 3116.64), (3, 3116.64), (4, 3116.64), (5, 3116.64), (6, 3116.64), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2654' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3026.18), (2, 3026.18), (3, 3026.18), (4, 3026.18), (5, 3026.18), (6, 3026.18), (7, 3116.66), (8, 3116.66), (9, 3116.66), (10, 3116.66), (11, 3116.66), (12, 3116.66)) v(Month, Amount)
WHERE c.PF_No = '2656' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.16), (2, 3207.16), (3, 3207.16), (4, 3207.16), (5, 3207.16), (6, 3207.16), (7, 3207.16), (8, 3207.16), (9, 3207.16), (10, 3207.16), (11, 3207.16), (12, 3207.16)) v(Month, Amount)
WHERE c.PF_No = '2657' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (2, 5013.36), (3, 5013.36), (4, 5013.36), (5, 5013.36), (6, 5013.36), (7, 5013.36), (8, 5013.36), (9, 5013.36), (10, 5013.36), (11, 5013.36), (12, 5013.36)) v(Month, Amount)
WHERE c.PF_No = '2665' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (10, 4629.74), (11, 4629.74), (12, 4629.74)) v(Month, Amount)
WHERE c.PF_No = '2675' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (2, 2935.68), (3, 2935.68), (4, 2935.68), (5, 2935.68), (6, 2935.68), (7, 3026.16), (8, 3026.16), (9, 3026.16), (10, 3026.16), (11, 3026.16), (12, 3026.16)) v(Month, Amount)
WHERE c.PF_No = '2680' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3340.74), (2, 3453.68), (3, 3453.68), (4, 3453.68), (5, 3453.68), (6, 3453.68), (7, 3453.68), (8, 3453.68), (9, 3453.68), (10, 3453.68), (11, 3453.68), (12, 3453.68)) v(Month, Amount)
WHERE c.PF_No = '2684' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3001.94), (2, 3114.86), (3, 3114.86), (4, 3114.86), (5, 3114.86), (6, 3114.86), (7, 3114.86), (8, 3114.86), (9, 3114.86), (10, 3114.86), (11, 3114.86), (12, 3114.86)) v(Month, Amount)
WHERE c.PF_No = '2685' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3566.6), (2, 3566.6)) v(Month, Amount)
WHERE c.PF_No = '2686' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3001.92), (2, 3001.92), (3, 3001.92), (4, 3001.92), (5, 3001.92), (6, 3001.92), (7, 3001.92), (8, 3001.92), (9, 3001.92), (10, 3001.92), (11, 3114.86), (12, 3114.86)) v(Month, Amount)
WHERE c.PF_No = '2690' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (10, 4131.26), (11, 4131.26), (12, 4131.26)) v(Month, Amount)
WHERE c.PF_No = '2692' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (2, 3114.86), (3, 3114.86), (4, 3114.86), (5, 3114.86), (6, 3114.86), (7, 3227.8), (8, 3227.8), (9, 3227.8), (10, 3227.8), (11, 3227.8), (12, 3227.8)) v(Month, Amount)
WHERE c.PF_No = '2696' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2033.34), (2, 2033.34), (3, 2033.34), (4, 2033.34), (5, 2033.34), (6, 2033.34), (7, 2109.84), (8, 2109.84), (9, 2109.84), (10, 2109.84), (11, 2109.84), (12, 2109.84)) v(Month, Amount)
WHERE c.PF_No = '3255' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2033.34), (2, 2033.34), (3, 2033.34), (4, 2033.34), (5, 2033.34), (6, 2033.34), (7, 2109.84), (8, 2109.84), (9, 2109.84), (10, 2109.84), (11, 2109.84), (12, 2109.84)) v(Month, Amount)
WHERE c.PF_No = '3280' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 1199.7), (2, 1199.7), (3, 1199.7), (4, 1199.7), (5, 1199.7), (6, 1199.7), (7, 1199.7), (8, 1199.7), (9, 1199.7), (10, 1199.7), (11, 1199.7), (12, 1199.7)) v(Month, Amount)
WHERE c.PF_No = '3306' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 1956.84), (2, 1956.84), (3, 1956.84), (4, 1956.84), (5, 1956.84), (6, 1956.84), (7, 1956.84), (8, 1956.84), (9, 1956.84), (10, 1956.84), (11, 1956.84), (12, 1956.84)) v(Month, Amount)
WHERE c.PF_No = '3334' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 1154.58), (2, 1154.58), (3, 1154.58), (4, 1154.58), (5, 1154.58), (6, 1154.58), (7, 1154.58), (8, 1154.58), (9, 1154.58)) v(Month, Amount)
WHERE c.PF_No = '3336' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2033.34), (2, 2033.34), (3, 2033.34), (4, 2033.34), (5, 2033.34), (6, 2033.34), (7, 2033.34), (8, 2033.34), (9, 2033.34), (10, 2033.34), (11, 2033.34), (12, 2033.34)) v(Month, Amount)
WHERE c.PF_No = '3337' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (6, 1956.84), (7, 1956.84), (8, 1956.84), (9, 1956.84), (10, 1956.84), (11, 1956.84), (12, 1956.84)) v(Month, Amount)
WHERE c.PF_No = '3343' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (6, 2109.84), (7, 2109.84), (8, 2109.84), (9, 2109.84), (10, 2109.84), (11, 2109.84), (12, 2109.84)) v(Month, Amount)
WHERE c.PF_No = '3344' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (4, 1956.84), (5, 1956.84), (6, 1956.84), (7, 1956.84), (8, 1956.84), (9, 1956.84), (10, 1956.84), (11, 1956.84), (12, 1956.84)) v(Month, Amount)
WHERE c.PF_No = '3348' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (10, 1956.84), (11, 1956.84), (12, 1956.84)) v(Month, Amount)
WHERE c.PF_No = '3351' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 4460.76)) v(Month, Amount)
WHERE c.PF_No = '2107' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 2754.72)) v(Month, Amount)
WHERE c.PF_No = '2515' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2517' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2523' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 3679.54)) v(Month, Amount)
WHERE c.PF_No = '2536' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 3116.66)) v(Month, Amount)
WHERE c.PF_No = '2537' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 4131.26)) v(Month, Amount)
WHERE c.PF_No = '2541' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 3734.62)) v(Month, Amount)
WHERE c.PF_No = '2554' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 3566.6)) v(Month, Amount)
WHERE c.PF_No = '2560' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 3207.16)) v(Month, Amount)
WHERE c.PF_No = '2568' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2636' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2659' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2673' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 3453.68)) v(Month, Amount)
WHERE c.PF_No = '2674' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 4131.26)) v(Month, Amount)
WHERE c.PF_No = '2678' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 2664.24)) v(Month, Amount)
WHERE c.PF_No = '2683' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 3679.54)) v(Month, Amount)
WHERE c.PF_No = '2686' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2695' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 2889.0)) v(Month, Amount)
WHERE c.PF_No = '2701' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 1956.84)) v(Month, Amount)
WHERE c.PF_No = '3340' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (12, 1956.84)) v(Month, Amount)
WHERE c.PF_No = '3350' AND r.Register_Year = 2024;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2304' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2313' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 5287.78), (2, 5287.78), (3, 5287.78), (4, 5287.78), (5, 5287.78), (6, 5287.78), (7, 5287.78), (8, 5287.78), (9, 5287.78), (10, 5287.78), (11, 5287.78), (12, 5287.78)) v(Month, Amount)
WHERE c.PF_No = '2314' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2426.88), (2, 2426.88), (3, 2426.88), (4, 2426.88), (5, 2426.88), (6, 2426.88), (7, 2426.88), (8, 2426.88), (9, 2426.88), (10, 2426.88), (11, 2426.88), (12, 2426.88)) v(Month, Amount)
WHERE c.PF_No = '2318' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 3754.2), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2323' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 1771.43), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2324' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 1771.43), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2325' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 1771.43), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2327' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2328' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 3754.2), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2331' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2332' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2333' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 1771.43), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2335' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 1771.43), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2339' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 3321.44), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2340' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4072.62), (2, 4072.62), (3, 4072.62), (4, 4072.62), (5, 4072.62), (6, 4072.62), (7, 4235.52), (8, 4235.52), (9, 2258.94), (10, 4235.52), (11, 4235.52), (12, 4235.52)) v(Month, Amount)
WHERE c.PF_No = '2346' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4072.62), (2, 4072.62), (3, 4072.62), (4, 4072.62), (5, 4072.62), (6, 4072.62), (7, 4072.62), (8, 4072.62), (9, 2172.06), (10, 4072.62), (11, 4072.62), (12, 4072.62)) v(Month, Amount)
WHERE c.PF_No = '2348' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 1771.43), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2349' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 6916.32), (2, 6916.32), (3, 6916.32), (4, 6916.32), (5, 6916.32), (6, 6916.32), (7, 6916.32), (8, 6916.32), (9, 3688.7), (10, 6916.32), (11, 6916.32), (12, 6916.32)) v(Month, Amount)
WHERE c.PF_No = '2401' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 5948.06), (2, 5948.06), (3, 5948.06), (4, 5948.06), (5, 5948.06), (6, 5948.06), (7, 5948.06), (8, 5948.06), (9, 3172.3), (10, 5948.06), (11, 5948.06), (12, 5948.06)) v(Month, Amount)
WHERE c.PF_No = '2402' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 6394.5), (2, 6394.5), (3, 6394.5), (4, 7425.91)) v(Month, Amount)
WHERE c.PF_No = '2405' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 6916.32), (2, 6916.32), (3, 6916.32), (4, 6916.32), (5, 6916.32), (6, 6916.32), (7, 6916.32), (8, 6916.32), (9, 6916.32), (10, 6916.32), (11, 6916.32), (12, 6916.32)) v(Month, Amount)
WHERE c.PF_No = '2406' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 1771.43), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2407' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4764.42), (2, 4764.42), (3, 4764.42), (4, 4764.42), (5, 4764.42), (6, 4764.42), (7, 4764.42), (8, 4764.42), (9, 4764.42), (10, 4764.42), (11, 4764.42), (12, 4764.42)) v(Month, Amount)
WHERE c.PF_No = '2501' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4072.62), (2, 4072.62), (3, 4072.62), (4, 4072.62), (5, 4072.62), (6, 4072.62), (7, 4235.52), (8, 4235.52), (9, 2258.94), (10, 4235.52), (11, 4235.52), (12, 4235.52)) v(Month, Amount)
WHERE c.PF_No = '2502' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4072.62), (2, 4072.62), (3, 4072.62), (4, 4072.62), (5, 4072.62), (6, 4072.62), (7, 4235.52), (8, 4235.52), (9, 4235.52), (10, 4235.52), (11, 4235.52), (12, 4235.52)) v(Month, Amount)
WHERE c.PF_No = '2504' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2505' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2506' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2513' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4072.62), (2, 4072.62), (3, 4072.62), (4, 4072.62), (5, 4072.62), (6, 4072.62), (7, 4072.62), (8, 4072.62), (9, 4072.62), (10, 4072.62), (11, 4072.62), (12, 4072.62)) v(Month, Amount)
WHERE c.PF_No = '2516' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2517' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2518' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3609.8), (2, 3609.8), (3, 3609.8), (4, 3609.8), (5, 3609.8), (6, 3609.8), (7, 3609.8), (8, 3609.8), (9, 1925.23), (10, 3609.8), (11, 3609.8), (12, 3609.8)) v(Month, Amount)
WHERE c.PF_No = '2519' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3609.8), (2, 3609.8), (3, 3609.8), (4, 3609.8), (5, 3609.8), (6, 3609.8), (7, 3609.8), (8, 3609.8), (9, 1925.23), (10, 3609.8), (11, 3609.8), (12, 3609.8)) v(Month, Amount)
WHERE c.PF_No = '2521' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 5948.06), (2, 5948.06), (3, 5948.06), (4, 5948.06), (5, 5948.06), (6, 5948.06), (7, 5948.06), (8, 5948.06), (9, 3172.3), (10, 5948.06), (11, 5948.06), (12, 5948.06)) v(Month, Amount)
WHERE c.PF_No = '2522' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (7, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2523' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2529' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2531' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2532' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2533' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 3754.2), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2535' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (10, 4235.52), (11, 4235.52), (12, 4235.52)) v(Month, Amount)
WHERE c.PF_No = '2536' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2537' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2538' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4764.42), (2, 4764.42), (3, 4764.42), (4, 4764.42), (5, 4764.42), (6, 4764.42), (7, 4764.42), (8, 4764.42), (9, 4764.42), (10, 4764.42), (11, 4764.42), (12, 4764.42)) v(Month, Amount)
WHERE c.PF_No = '2539' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4764.42), (2, 4764.42), (3, 4764.42), (4, 4764.42), (5, 4764.42), (6, 4764.42), (7, 4764.42), (8, 4764.42), (9, 2541.02), (10, 4764.42), (11, 4764.42), (12, 4764.42)) v(Month, Amount)
WHERE c.PF_No = '2540' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (6, 4764.42), (7, 4764.42), (8, 4764.42), (9, 2541.02), (10, 4764.42), (11, 4764.42), (12, 4764.42)) v(Month, Amount)
WHERE c.PF_No = '2541' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 5084.4), (2, 5084.4), (3, 5084.4), (4, 5084.4), (5, 5084.4), (6, 5084.4), (7, 5287.78), (8, 5287.78), (9, 5287.78), (10, 5287.78), (11, 5287.78), (12, 5287.78)) v(Month, Amount)
WHERE c.PF_No = '2542' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2543' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 3754.2), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2547' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2550' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (10, 4178.94), (11, 4178.94), (12, 4178.94)) v(Month, Amount)
WHERE c.PF_No = '2554' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4764.42), (2, 4764.42), (3, 4764.42), (4, 4764.42), (5, 4764.42), (6, 4764.42), (7, 4764.42), (8, 4764.42), (9, 2541.02), (10, 4764.42), (11, 4764.42), (12, 4764.42)) v(Month, Amount)
WHERE c.PF_No = '2555' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (10, 4072.62), (11, 4072.62), (12, 4072.62)) v(Month, Amount)
WHERE c.PF_No = '2560' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2536.1), (2, 2536.1), (3, 2536.1), (4, 2536.1), (5, 2536.1), (6, 2536.1), (7, 2536.1), (8, 2536.1), (9, 2536.1), (10, 2536.1), (11, 2536.1), (12, 2536.1)) v(Month, Amount)
WHERE c.PF_No = '2565' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2333.5), (2, 2333.5), (3, 2333.5), (4, 2333.5), (5, 2333.5), (6, 2333.5), (7, 2333.5), (8, 2333.5), (9, 1294.34), (10, 2426.88), (11, 2426.88), (12, 2426.88)) v(Month, Amount)
WHERE c.PF_No = '2567' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (5, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2568' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2587' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 1771.43), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2613' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3915.96), (2, 3915.96), (3, 3915.96), (4, 3915.96), (5, 3915.96), (6, 3915.96), (7, 4072.62), (8, 4072.62), (9, 4072.62), (10, 4072.62), (11, 4072.62), (12, 4072.62)) v(Month, Amount)
WHERE c.PF_No = '2614' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3609.8), (2, 3609.8), (3, 3609.8), (4, 3609.8), (5, 3609.8), (6, 3609.8), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2616' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2620' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2621' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 3754.2), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2622' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3609.8), (2, 3609.8), (3, 3609.8), (4, 3609.8), (5, 3609.8), (6, 3609.8), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2624' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3609.8), (2, 3609.8), (3, 3609.8), (4, 3609.8), (5, 3609.8), (6, 3609.8), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2625' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 1771.43), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2626' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 1771.43), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2627' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 3754.2), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2628' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 3321.44), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2629' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2631' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3609.8), (2, 3609.8), (3, 3609.8), (4, 3609.8), (5, 3609.8), (6, 3609.8), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2632' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3609.8), (2, 3609.8), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2634' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3321.44), (2, 3321.44), (3, 3321.44), (4, 3321.44), (5, 3321.44), (6, 3321.44), (7, 3321.44), (8, 3321.44), (9, 1771.43), (10, 3321.44), (11, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2635' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3915.96), (2, 3915.96), (3, 3915.96), (4, 3915.96), (5, 3915.96), (6, 3915.96), (7, 4072.62), (8, 4072.62), (9, 2172.06), (10, 4072.62), (11, 4072.62), (12, 4072.62)) v(Month, Amount)
WHERE c.PF_No = '2637' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2639' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 5948.06), (2, 5948.06), (3, 5948.06), (4, 5948.06), (5, 5948.06), (6, 5948.06), (7, 5948.06), (8, 5948.06), (9, 5948.06), (10, 5948.06), (11, 5948.06), (12, 5948.06)) v(Month, Amount)
WHERE c.PF_No = '2642' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2644' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 3754.2), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2646' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3609.8), (2, 3609.8), (3, 3609.8), (4, 3609.8), (5, 3609.8), (6, 3609.8), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2650' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3609.8), (2, 3609.8), (3, 3609.8), (4, 3609.8), (5, 3609.8), (6, 3609.8), (7, 3754.2), (8, 3754.2), (9, 3754.2), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2651' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 3754.2), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2653' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2654' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3609.8), (2, 3609.8), (3, 3609.8), (4, 3609.8), (5, 3609.8), (6, 3609.8), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2656' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3754.2), (2, 3754.2), (3, 3754.2), (4, 3754.2), (5, 3754.2), (6, 3754.2), (7, 3754.2), (8, 3754.2), (9, 2002.24), (10, 3754.2), (11, 3754.2), (12, 3754.2)) v(Month, Amount)
WHERE c.PF_No = '2657' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (7, 3321.44), (12, 3321.44)) v(Month, Amount)
WHERE c.PF_No = '2659' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 5948.06), (2, 5948.06), (3, 5948.06), (4, 7217.57)) v(Month, Amount)
WHERE c.PF_No = '2665' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (7, 2798.3), (8, 2798.3), (9, 2798.3), (10, 2798.3), (11, 2798.3), (12, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2673' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (6, 3765.34), (7, 3915.96), (8, 3915.96), (9, 3915.96), (10, 3915.96), (11, 3915.96), (12, 3915.96)) v(Month, Amount)
WHERE c.PF_No = '2674' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 5287.78), (2, 5287.78), (3, 5287.78), (4, 5287.78), (5, 5287.78), (6, 5287.78), (7, 5499.3), (8, 5499.3), (9, 5499.3), (10, 5499.3), (11, 5499.3), (12, 5499.3)) v(Month, Amount)
WHERE c.PF_No = '2675' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3470.96), (2, 3470.96), (3, 3470.96), (4, 3470.96), (5, 3470.96), (6, 3470.96), (7, 3609.8), (8, 3609.8), (9, 1925.23), (10, 3609.8), (11, 3609.8), (12, 3609.8)) v(Month, Amount)
WHERE c.PF_No = '2680' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3085.62), (2, 3085.62), (3, 3085.62), (4, 3085.62), (5, 3085.62), (6, 3085.62)) v(Month, Amount)
WHERE c.PF_No = '2683' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3915.96), (2, 3915.96), (3, 3915.96), (4, 3915.96), (5, 3915.96), (6, 3915.96), (7, 3915.96), (8, 3915.96), (9, 3915.96), (10, 3915.96), (11, 3915.96), (12, 3915.96)) v(Month, Amount)
WHERE c.PF_No = '2684' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3481.26), (2, 3481.26), (3, 3481.26), (4, 3481.26), (5, 3481.26), (6, 3481.26), (7, 3481.26), (8, 3481.26), (9, 3481.26), (10, 3481.26), (11, 3481.26), (12, 3481.26)) v(Month, Amount)
WHERE c.PF_No = '2685' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (10, 4404.94), (11, 4404.94), (12, 4404.94)) v(Month, Amount)
WHERE c.PF_No = '2686' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3347.34), (2, 3347.34), (3, 3347.34), (4, 3347.34), (5, 3347.34), (6, 3347.34), (7, 3481.26), (8, 3481.26), (9, 3481.26), (10, 3481.26), (11, 3481.26), (12, 3481.26)) v(Month, Amount)
WHERE c.PF_No = '2690' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4764.42), (2, 4764.42), (3, 4764.42), (4, 4764.42), (5, 4764.42), (6, 4764.42), (7, 4764.42), (8, 4764.42), (9, 2541.02), (10, 4764.42), (11, 4764.42), (12, 4764.42)) v(Month, Amount)
WHERE c.PF_No = '2692' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3481.26), (2, 3481.26), (3, 3481.26), (4, 3481.26), (5, 3481.26), (6, 3481.26), (7, 3620.5), (8, 3620.5), (9, 1930.93), (10, 3620.5), (11, 3620.5), (12, 3620.5)) v(Month, Amount)
WHERE c.PF_No = '2696' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (5, 3218.58)) v(Month, Amount)
WHERE c.PF_No = '2701' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (5, 2852.8), (10, 2852.8), (11, 2852.8), (12, 2852.8)) v(Month, Amount)
WHERE c.PF_No = '2703' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (9, 1300.57), (10, 2438.56), (11, 2438.56), (12, 2438.56)) v(Month, Amount)
WHERE c.PF_No = '2704' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (9, 1521.49), (10, 2852.8), (11, 2852.8), (12, 2852.8)) v(Month, Amount)
WHERE c.PF_No = '2705' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (9, 693.63), (10, 2438.56), (11, 2438.56), (12, 2438.56)) v(Month, Amount)
WHERE c.PF_No = '2706' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (4, 1050.88)) v(Month, Amount)
WHERE c.PF_No = '3111' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2333.5), (2, 2333.5), (3, 2333.5), (4, 2333.5), (5, 2333.5), (6, 2333.5), (7, 2426.88), (8, 2426.88), (9, 1294.34), (10, 2426.88), (11, 2426.88), (12, 2426.88)) v(Month, Amount)
WHERE c.PF_No = '3255' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2333.5), (2, 2333.5), (3, 2333.5), (4, 2333.5), (5, 2333.5), (6, 2333.5), (7, 2426.88), (8, 2426.88), (9, 1294.34), (10, 2426.88), (11, 2426.88), (12, 2426.88)) v(Month, Amount)
WHERE c.PF_No = '3280' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 1323.88), (2, 1323.88), (3, 1323.88), (4, 1323.88), (5, 1323.88), (6, 1323.88), (7, 1323.88), (8, 1323.88), (9, 706.07), (10, 1323.88), (11, 1323.88), (12, 1323.88)) v(Month, Amount)
WHERE c.PF_No = '3306' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2243.74), (2, 2243.74), (3, 2243.74), (4, 2243.74), (5, 2243.74), (6, 2243.74), (7, 2243.74), (8, 2243.74), (9, 1196.66), (10, 2243.74), (11, 2243.74), (12, 2243.74)) v(Month, Amount)
WHERE c.PF_No = '3334' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2243.74), (2, 2243.74), (3, 2243.74), (4, 2243.74), (5, 2243.74), (6, 2243.74), (7, 2243.74), (8, 2243.74), (9, 1196.66), (10, 2243.74), (11, 2243.74), (12, 2243.74)) v(Month, Amount)
WHERE c.PF_No = '3337' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2157.44), (2, 2157.44), (3, 2157.44), (4, 2157.44), (5, 2157.44), (6, 2157.44), (7, 2157.44), (8, 2157.44), (9, 1150.63), (10, 2157.44), (11, 2157.44), (12, 2157.44)) v(Month, Amount)
WHERE c.PF_No = '3343' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2333.5), (2, 2333.5), (3, 2333.5), (4, 2333.5), (5, 2333.5), (6, 2333.5), (7, 2333.5), (8, 2333.5), (9, 1244.53), (10, 2333.5), (11, 2333.5), (12, 2333.5)) v(Month, Amount)
WHERE c.PF_No = '3344' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2157.44), (2, 2157.44), (3, 2157.44), (4, 2157.44), (5, 2157.44), (6, 2157.44), (7, 2157.44), (8, 2157.44), (9, 1150.63), (10, 2157.44), (11, 2157.44), (12, 2157.44)) v(Month, Amount)
WHERE c.PF_No = '3348' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Member', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2157.44), (2, 2157.44), (3, 2157.44), (4, 2157.44), (5, 2157.44), (6, 2157.44), (7, 2157.44), (8, 2157.44), (9, 1150.63), (10, 2157.44), (11, 2157.44), (12, 2157.44)) v(Month, Amount)
WHERE c.PF_No = '3351' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4460.76), (2, 4460.76), (3, 4460.76), (4, 4460.76), (5, 4460.76), (6, 4460.76), (7, 4460.76), (8, 4460.76), (9, 4460.76), (10, 4460.76), (11, 4460.76), (12, 4460.76)) v(Month, Amount)
WHERE c.PF_No = '2107' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2517' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2523' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3679.54), (2, 3679.54), (3, 3679.54), (4, 3679.54), (5, 3679.54), (6, 3679.54), (7, 3679.54), (8, 3679.54), (9, 3679.54)) v(Month, Amount)
WHERE c.PF_No = '2536' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3116.66), (2, 3116.66), (3, 3116.66), (4, 3116.66), (5, 3116.66), (6, 3116.66), (7, 3116.66), (8, 3116.66)) v(Month, Amount)
WHERE c.PF_No = '2537' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4131.26), (2, 4131.26), (3, 4131.26), (4, 4131.26), (5, 4131.26)) v(Month, Amount)
WHERE c.PF_No = '2541' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3734.62), (2, 3734.62), (3, 3734.62), (4, 3734.62), (5, 3734.62), (6, 3734.62), (7, 3734.62), (8, 3734.62), (9, 3734.62)) v(Month, Amount)
WHERE c.PF_No = '2554' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3566.6), (2, 3566.6), (3, 3566.6), (4, 3566.6), (5, 3566.6), (6, 3566.6), (7, 3566.6), (8, 3566.6), (9, 3566.6)) v(Month, Amount)
WHERE c.PF_No = '2560' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.16), (2, 3207.16), (3, 3207.16), (4, 3207.16)) v(Month, Amount)
WHERE c.PF_No = '2568' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2636' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2659' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2798.3), (2, 2798.3), (3, 2798.3), (4, 2798.3), (5, 2798.3), (6, 2798.3)) v(Month, Amount)
WHERE c.PF_No = '2673' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3453.68), (2, 3453.68), (3, 3453.68), (4, 3453.68), (5, 3453.68)) v(Month, Amount)
WHERE c.PF_No = '2674' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 4131.26), (2, 4131.26), (3, 4131.26), (4, 4131.26), (5, 4131.26), (6, 4131.26), (7, 4131.26), (8, 4131.26), (9, 4131.26), (10, 4131.26), (11, 4131.26), (12, 4131.26)) v(Month, Amount)
WHERE c.PF_No = '2678' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3679.54), (2, 3679.54), (3, 3679.54), (4, 3679.54), (5, 3679.54), (6, 3679.54), (7, 3679.54), (8, 3679.54), (9, 3679.54)) v(Month, Amount)
WHERE c.PF_No = '2686' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 3207.14), (2, 3207.14), (3, 3207.14), (4, 3207.14), (5, 3207.14), (6, 3207.14), (7, 3207.14), (8, 3207.14), (9, 3207.14), (10, 3207.14), (11, 3207.14), (12, 3207.14)) v(Month, Amount)
WHERE c.PF_No = '2695' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 2889.0), (2, 2889.0), (3, 2889.0), (4, 2889.0)) v(Month, Amount)
WHERE c.PF_No = '2701' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (8, 1956.84), (9, 1956.84), (10, 1956.84), (11, 1956.84), (12, 1956.84)) v(Month, Amount)
WHERE c.PF_No = '3340' AND r.Register_Year = 2025;
INSERT INTO Union_Contributions (Registration_ID, Contribution_Month, Membership_Type, Amount)
SELECT r.Registration_ID, v.Month, 'Agency', v.Amount
FROM Union_Registrations r
INNER JOIN Union_Contributors c ON c.Contributor_ID = r.Contributor_ID
CROSS APPLY (VALUES (1, 1956.84), (2, 1956.84), (3, 1956.84), (4, 1956.84), (5, 1956.84), (6, 1956.84), (7, 1956.84), (8, 1956.84), (9, 1956.84), (10, 1956.84), (11, 1956.84), (12, 1956.84)) v(Month, Amount)
WHERE c.PF_No = '3350' AND r.Register_Year = 2025;
GO
