EESchema Schematic File Version 4
EELAYER 30 0
EELAYER END
$Descr A4 11693 8268
encoding utf-8
Sheet 1 1
Title ""
Date ""
Rev ""
Comp ""
Comment1 ""
Comment2 ""
Comment3 ""
Comment4 ""
$EndDescr
$Comp
L Switch:SW_SPDT SW1
U 1 1 618CAB46
P 1700 900
F 0 "SW1" H 1700 1185 50  0000 C CNN
F 1 "SW_SPDT" H 1700 1094 50  0000 C CNN
F 2 "mClara:SW_PCM12SMTR" H 1700 900 50  0001 C CNN
F 3 "https://www.ckswitches.com/media/1424/pcm.pdf" H 1700 900 50  0001 C CNN
F 4 "PCM12SMTR" H 1700 900 50  0001 C CNN "MPN"
	1    1700 900 
	1    0    0    -1  
$EndComp
$Comp
L Device:C C1
U 1 1 618CCB78
P 2250 1500
F 0 "C1" H 2365 1546 50  0000 L CNN
F 1 "47uF" H 2365 1455 50  0000 L CNN
F 2 "Capacitor_SMD:C_1206_3216Metric_Pad1.33x1.80mm_HandSolder" H 2288 1350 50  0001 C CNN
F 3 "http://www.samsungsem.com/kr/support/product-search/mlcc/CL31A476MQHNNWE.jsp" H 2250 1500 50  0001 C CNN
F 4 "CL31A476MQHNNWE" H 2250 1500 50  0001 C CNN "MPN"
	1    2250 1500
	1    0    0    -1  
$EndComp
$Comp
L Device:C C2
U 1 1 618CE206
P 4500 1500
F 0 "C2" H 4615 1546 50  0000 L CNN
F 1 "47uF" H 4615 1455 50  0000 L CNN
F 2 "Capacitor_SMD:C_1206_3216Metric_Pad1.33x1.80mm_HandSolder" H 4538 1350 50  0001 C CNN
F 3 "http://www.samsungsem.com/kr/support/product-search/mlcc/CL31A476MQHNNWE.jsp" H 4500 1500 50  0001 C CNN
F 4 "CL31A476MQHNNWE" H 4500 1500 50  0001 C CNN "MPN"
	1    4500 1500
	1    0    0    -1  
$EndComp
$Comp
L mClara:AP1603WG-7 U1
U 1 1 618CF1A2
P 3500 1200
F 0 "U1" H 3500 1150 50  0000 C CNN
F 1 "AP1603WG-7" H 3500 650 50  0000 C CNN
F 2 "Package_TO_SOT_SMD:SOT-23-6_Handsoldering" H 3500 1200 50  0001 C CNN
F 3 "https://www.diodes.com/assets/Datasheets/AP1603.pdf" H 3500 1200 50  0001 C CNN
F 4 "AP1603WG-7" H 3500 1200 50  0001 C CNN "MPN"
	1    3500 1200
	1    0    0    -1  
$EndComp
$Comp
L Device:L L1
U 1 1 618D0707
P 3500 1000
F 0 "L1" V 3690 1000 50  0000 C CNN
F 1 "22uH" V 3599 1000 50  0000 C CNN
F 2 "Inductor_SMD:L_1812_4532Metric_Pad1.30x3.40mm_HandSolder" H 3500 1000 50  0001 C CNN
F 3 "https://search.murata.co.jp/Ceramy/image/img/P02/JELF243A-0093.pdf" H 3500 1000 50  0001 C CNN
F 4 "LQH43PN220M26L" H 3500 1000 50  0001 C CNN "MPN"
	1    3500 1000
	0    -1   -1   0   
$EndComp
NoConn ~ 1900 800 
Wire Wire Line
	1000 2000 2250 2000
$Comp
L power:GND #PWR0101
U 1 1 618D8FD0
P 1000 2000
F 0 "#PWR0101" H 1000 1750 50  0001 C CNN
F 1 "GND" H 1005 1827 50  0000 C CNN
F 2 "" H 1000 2000 50  0001 C CNN
F 3 "" H 1000 2000 50  0001 C CNN
	1    1000 2000
	1    0    0    -1  
$EndComp
Wire Wire Line
	3650 1000 4000 1000
Wire Wire Line
	4000 1000 4000 1600
Wire Wire Line
	4000 1600 3850 1600
Wire Wire Line
	3850 1500 3850 1400
Wire Wire Line
	3850 1400 3850 1100
Wire Wire Line
	3850 1100 3150 1100
Wire Wire Line
	3150 1100 3150 1400
Connection ~ 3850 1400
Wire Wire Line
	3850 1100 4500 1100
Wire Wire Line
	4500 1100 4500 1350
Connection ~ 3850 1100
$Comp
L Device:C C3
U 1 1 618DBE9F
P 2800 1800
F 0 "C3" H 2915 1846 50  0000 L CNN
F 1 "100nF" H 2915 1755 50  0000 L CNN
F 2 "Capacitor_SMD:C_0603_1608Metric_Pad1.08x0.95mm_HandSolder" H 2838 1650 50  0001 C CNN
F 3 "http://www.samsungsem.com/kr/support/product-search/mlcc/CL10B104KB8NNWC.jsp" H 2800 1800 50  0001 C CNN
F 4 "CL10B104KB8NNWC" H 2800 1800 50  0001 C CNN "MPN"
	1    2800 1800
	1    0    0    -1  
$EndComp
Wire Wire Line
	2250 1350 2250 1000
Connection ~ 2250 1000
Wire Wire Line
	2250 1650 2250 2000
Wire Wire Line
	2250 1000 2500 1000
Wire Wire Line
	3150 1600 2800 1600
Wire Wire Line
	2800 1600 2800 1650
Wire Wire Line
	3150 1500 2600 1500
Wire Wire Line
	2600 1500 2600 2000
Wire Wire Line
	2600 2000 2250 2000
Connection ~ 2250 2000
Wire Wire Line
	2600 2000 2800 2000
Wire Wire Line
	2800 2000 2800 1950
Connection ~ 2600 2000
Wire Wire Line
	2800 2000 4500 2000
Wire Wire Line
	4500 2000 4500 1650
Connection ~ 2800 2000
$Comp
L power:+3.3V #PWR0102
U 1 1 618E45EE
P 4500 1100
F 0 "#PWR0102" H 4500 950 50  0001 C CNN
F 1 "+3.3V" H 4515 1273 50  0000 C CNN
F 2 "" H 4500 1100 50  0001 C CNN
F 3 "" H 4500 1100 50  0001 C CNN
	1    4500 1100
	1    0    0    -1  
$EndComp
Connection ~ 4500 1100
$Comp
L Timer:LM555xM U2
U 1 1 618E59B8
P 2250 3050
F 0 "U2" H 2550 3400 50  0000 C CNN
F 1 "LM555xM" H 2450 2700 50  0000 C CNN
F 2 "Package_SO:SOIC-8_3.9x4.9mm_P1.27mm" H 3100 2650 50  0001 C CNN
F 3 "https://www.ti.com/lit/ds/symlink/tlc555.pdf" H 3100 2650 50  0001 C CNN
F 4 "TLC555IDR" H 2250 3050 50  0001 C CNN "MPN"
	1    2250 3050
	1    0    0    -1  
$EndComp
$Comp
L power:+3.3V #PWR0103
U 1 1 618E6CDA
P 2250 2650
F 0 "#PWR0103" H 2250 2500 50  0001 C CNN
F 1 "+3.3V" H 2265 2823 50  0000 C CNN
F 2 "" H 2250 2650 50  0001 C CNN
F 3 "" H 2250 2650 50  0001 C CNN
	1    2250 2650
	1    0    0    -1  
$EndComp
$Comp
L power:GND #PWR0104
U 1 1 618E8715
P 2250 3450
F 0 "#PWR0104" H 2250 3200 50  0001 C CNN
F 1 "GND" H 2255 3277 50  0000 C CNN
F 2 "" H 2250 3450 50  0001 C CNN
F 3 "" H 2250 3450 50  0001 C CNN
	1    2250 3450
	1    0    0    -1  
$EndComp
$Comp
L Device:R R1
U 1 1 618E90F9
P 1000 2650
F 0 "R1" H 1070 2696 50  0000 L CNN
F 1 "180k" H 1070 2605 50  0000 L CNN
F 2 "Resistor_SMD:R_0603_1608Metric_Pad0.98x0.95mm_HandSolder" V 930 2650 50  0001 C CNN
F 3 "https://www.seielect.com/catalog/sei-rmcf_rmcp.pdf" H 1000 2650 50  0001 C CNN
F 4 "RMCF0603FT180K" H 1000 2650 50  0001 C CNN "MPN"
	1    1000 2650
	1    0    0    -1  
$EndComp
$Comp
L Device:C C6
U 1 1 618EAC09
P 1000 3050
F 0 "C6" H 1115 3096 50  0000 L CNN
F 1 "1uF" H 1115 3005 50  0000 L CNN
F 2 "Capacitor_SMD:C_0603_1608Metric_Pad1.08x0.95mm_HandSolder" H 1038 2900 50  0001 C CNN
F 3 "https://media.digikey.com/pdf/Data%20Sheets/Samsung%20PDFs/CL10A105KO8NNNC_Spec.pdf" H 1000 3050 50  0001 C CNN
F 4 "CL10A105KO8NNNC" H 1000 3050 50  0001 C CNN "MPN"
	1    1000 3050
	1    0    0    -1  
$EndComp
Wire Wire Line
	2750 2850 2750 2400
Wire Wire Line
	1000 2400 1000 2500
Wire Wire Line
	1000 2800 1000 2850
Connection ~ 1000 2850
Wire Wire Line
	1000 2850 1000 2900
Wire Wire Line
	2750 3700 2750 3250
$Comp
L power:GND #PWR0105
U 1 1 618EE131
P 1000 3200
F 0 "#PWR0105" H 1000 2950 50  0001 C CNN
F 1 "GND" H 1005 3027 50  0000 C CNN
F 2 "" H 1000 3200 50  0001 C CNN
F 3 "" H 1000 3200 50  0001 C CNN
	1    1000 3200
	1    0    0    -1  
$EndComp
NoConn ~ 1750 3050
$Comp
L power:+3.3V #PWR0106
U 1 1 618EEEB7
P 1750 3250
F 0 "#PWR0106" H 1750 3100 50  0001 C CNN
F 1 "+3.3V" V 1765 3378 50  0000 L CNN
F 2 "" H 1750 3250 50  0001 C CNN
F 3 "" H 1750 3250 50  0001 C CNN
	1    1750 3250
	0    -1   -1   0   
$EndComp
Wire Wire Line
	1000 2400 2750 2400
Wire Wire Line
	1000 2850 1350 2850
Wire Wire Line
	1350 2850 1350 3700
Wire Wire Line
	1350 3700 2750 3700
Connection ~ 1350 2850
Wire Wire Line
	1350 2850 1750 2850
$Comp
L Device:C C4
U 1 1 618F2987
P 3000 3050
F 0 "C4" H 3115 3096 50  0000 L CNN
F 1 "100nF" H 3115 3005 50  0000 L CNN
F 2 "Capacitor_SMD:C_0603_1608Metric_Pad1.08x0.95mm_HandSolder" H 3038 2900 50  0001 C CNN
F 3 "http://www.samsungsem.com/kr/support/product-search/mlcc/CL10B104KB8NNWC.jsp" H 3000 3050 50  0001 C CNN
F 4 "CL10B104KB8NNWC" H 3000 3050 50  0001 C CNN "MPN"
	1    3000 3050
	1    0    0    -1  
$EndComp
Wire Wire Line
	2250 2650 3000 2650
Wire Wire Line
	3000 2650 3000 2900
Connection ~ 2250 2650
Wire Wire Line
	3000 3200 3000 3450
Wire Wire Line
	3000 3450 2250 3450
Connection ~ 2250 3450
NoConn ~ 2750 3050
Text Label 2750 2400 0    50   ~ 0
CLK
$Comp
L 74HC4017D_653:74HC4017D,653 U3
U 1 1 618F9D2B
P 4800 3000
F 0 "U3" H 4800 3770 50  0000 C CNN
F 1 "74HC4017D,653" H 4800 3679 50  0000 C CNN
F 2 "Package_SO:SO-16_3.9x9.9mm_P1.27mm" H 4800 3000 50  0001 L BNN
F 3 "https://assets.nexperia.com/documents/data-sheet/74HC_HCT4017.pdf" H 4800 3000 50  0001 L BNN
F 4 "" H 4800 3000 50  0001 L BNN "OC_FARNELL"
F 5 "" H 4800 3000 50  0001 L BNN "OC_NEWARK"
F 6 "74HC4017D,653" H 4800 3000 50  0001 L BNN "MPN"
F 7 "" H 4800 3000 50  0001 L BNN "PACKAGE"
F 8 "" H 4800 3000 50  0001 L BNN "SUPPLIER"
	1    4800 3000
	1    0    0    -1  
$EndComp
$Comp
L Device:C C5
U 1 1 618FCAC0
P 3500 3050
F 0 "C5" H 3615 3096 50  0000 L CNN
F 1 "100nF" H 3615 3005 50  0000 L CNN
F 2 "Capacitor_SMD:C_0603_1608Metric_Pad1.08x0.95mm_HandSolder" H 3538 2900 50  0001 C CNN
F 3 "http://www.samsungsem.com/kr/support/product-search/mlcc/CL10B104KB8NNWC.jsp" H 3500 3050 50  0001 C CNN
F 4 "CL10B104KB8NNWC" H 3500 3050 50  0001 C CNN "MPN"
	1    3500 3050
	1    0    0    -1  
$EndComp
$Comp
L power:+3.3V #PWR0107
U 1 1 618FE6C8
P 4100 2600
F 0 "#PWR0107" H 4100 2450 50  0001 C CNN
F 1 "+3.3V" H 4115 2773 50  0000 C CNN
F 2 "" H 4100 2600 50  0001 C CNN
F 3 "" H 4100 2600 50  0001 C CNN
	1    4100 2600
	1    0    0    -1  
$EndComp
Text Label 4100 2900 2    50   ~ 0
CLK
NoConn ~ 4100 2800
NoConn ~ 5500 3700
$Comp
L power:GND #PWR0108
U 1 1 619001E8
P 4100 3200
F 0 "#PWR0108" H 4100 2950 50  0001 C CNN
F 1 "GND" H 4105 3027 50  0000 C CNN
F 2 "" H 4100 3200 50  0001 C CNN
F 3 "" H 4100 3200 50  0001 C CNN
	1    4100 3200
	1    0    0    -1  
$EndComp
Wire Wire Line
	5500 3600 5600 3600
Wire Wire Line
	5600 3600 5600 4000
Wire Wire Line
	5600 4000 3950 4000
Wire Wire Line
	3950 3000 3950 4000
Wire Wire Line
	3950 3000 4100 3000
NoConn ~ 5500 2600
Wire Wire Line
	3500 3200 4100 3200
Connection ~ 4100 3200
Wire Wire Line
	4100 2600 3500 2600
Wire Wire Line
	3500 2600 3500 2900
Connection ~ 4100 2600
Text Label 5500 2800 0    50   ~ 0
LED1
Text Label 5500 2900 0    50   ~ 0
LED2
Text Label 5500 3000 0    50   ~ 0
LED3
Text Label 5500 3100 0    50   ~ 0
LED4
Text Label 5500 3200 0    50   ~ 0
LED5
Text Label 5500 3300 0    50   ~ 0
LED6
Text Label 5500 3400 0    50   ~ 0
LED7
$Comp
L Device:LED D1
U 1 1 61907601
P 1000 4650
F 0 "D1" V 1039 4532 50  0000 R CNN
F 1 "LED" V 948 4532 50  0000 R CNN
F 2 "LED_SMD:LED_0603_1608Metric_Pad1.05x0.95mm_HandSolder" H 1000 4650 50  0001 C CNN
F 3 "http://www.inolux-corp.com/datasheet/SMDLED/Mono%20Color%20Top%20View/IN-S63AT%20Series_V1.1.pdf" H 1000 4650 50  0001 C CNN
F 4 "IN-S63AT5UW" H 1000 4650 50  0001 C CNN "MPN"
	1    1000 4650
	0    -1   -1   0   
$EndComp
$Comp
L Device:LED D2
U 1 1 6190AC0E
P 1500 4650
F 0 "D2" V 1539 4532 50  0000 R CNN
F 1 "LED" V 1448 4532 50  0000 R CNN
F 2 "LED_SMD:LED_0603_1608Metric_Pad1.05x0.95mm_HandSolder" H 1500 4650 50  0001 C CNN
F 3 "http://www.inolux-corp.com/datasheet/SMDLED/Mono%20Color%20Top%20View/IN-S63AT%20Series_V1.1.pdf" H 1500 4650 50  0001 C CNN
F 4 "IN-S63AT5UW" H 1500 4650 50  0001 C CNN "MPN"
	1    1500 4650
	0    -1   -1   0   
$EndComp
$Comp
L Device:LED D3
U 1 1 6190B473
P 2000 4650
F 0 "D3" V 2039 4532 50  0000 R CNN
F 1 "LED" V 1948 4532 50  0000 R CNN
F 2 "LED_SMD:LED_0603_1608Metric_Pad1.05x0.95mm_HandSolder" H 2000 4650 50  0001 C CNN
F 3 "http://www.inolux-corp.com/datasheet/SMDLED/Mono%20Color%20Top%20View/IN-S63AT%20Series_V1.1.pdf" H 2000 4650 50  0001 C CNN
F 4 "IN-S63AT5UW" H 2000 4650 50  0001 C CNN "MPN"
	1    2000 4650
	0    -1   -1   0   
$EndComp
$Comp
L Device:LED D4
U 1 1 6190B908
P 2500 4650
F 0 "D4" V 2539 4532 50  0000 R CNN
F 1 "LED" V 2448 4532 50  0000 R CNN
F 2 "LED_SMD:LED_0603_1608Metric_Pad1.05x0.95mm_HandSolder" H 2500 4650 50  0001 C CNN
F 3 "http://www.inolux-corp.com/datasheet/SMDLED/Mono%20Color%20Top%20View/IN-S63AT%20Series_V1.1.pdf" H 2500 4650 50  0001 C CNN
F 4 "IN-S63AT5UW" H 2500 4650 50  0001 C CNN "MPN"
	1    2500 4650
	0    -1   -1   0   
$EndComp
$Comp
L Device:LED D5
U 1 1 6190BDC4
P 3000 4650
F 0 "D5" V 3039 4532 50  0000 R CNN
F 1 "LED" V 2948 4532 50  0000 R CNN
F 2 "LED_SMD:LED_0603_1608Metric_Pad1.05x0.95mm_HandSolder" H 3000 4650 50  0001 C CNN
F 3 "http://www.inolux-corp.com/datasheet/SMDLED/Mono%20Color%20Top%20View/IN-S63AT%20Series_V1.1.pdf" H 3000 4650 50  0001 C CNN
F 4 "IN-S63AT5UW" H 3000 4650 50  0001 C CNN "MPN"
	1    3000 4650
	0    -1   -1   0   
$EndComp
$Comp
L Device:LED D6
U 1 1 61916046
P 3500 4650
F 0 "D6" V 3539 4532 50  0000 R CNN
F 1 "LED" V 3448 4532 50  0000 R CNN
F 2 "LED_SMD:LED_0603_1608Metric_Pad1.05x0.95mm_HandSolder" H 3500 4650 50  0001 C CNN
F 3 "http://www.inolux-corp.com/datasheet/SMDLED/Mono%20Color%20Top%20View/IN-S63AT%20Series_V1.1.pdf" H 3500 4650 50  0001 C CNN
F 4 "IN-S63AT5UW" H 3500 4650 50  0001 C CNN "MPN"
	1    3500 4650
	0    -1   -1   0   
$EndComp
$Comp
L Device:LED D7
U 1 1 61916396
P 4000 4650
F 0 "D7" V 4039 4532 50  0000 R CNN
F 1 "LED" V 3948 4532 50  0000 R CNN
F 2 "LED_SMD:LED_0603_1608Metric_Pad1.05x0.95mm_HandSolder" H 4000 4650 50  0001 C CNN
F 3 "http://www.inolux-corp.com/datasheet/SMDLED/Mono%20Color%20Top%20View/IN-S63AT%20Series_V1.1.pdf" H 4000 4650 50  0001 C CNN
F 4 "IN-S63AT5UW" H 4000 4650 50  0001 C CNN "MPN"
	1    4000 4650
	0    -1   -1   0   
$EndComp
$Comp
L Device:LED D8
U 1 1 61916672
P 4500 4650
F 0 "D8" V 4539 4532 50  0000 R CNN
F 1 "LED" V 4448 4532 50  0000 R CNN
F 2 "LED_SMD:LED_0603_1608Metric_Pad1.05x0.95mm_HandSolder" H 4500 4650 50  0001 C CNN
F 3 "http://www.inolux-corp.com/datasheet/SMDLED/Mono%20Color%20Top%20View/IN-S63AT%20Series_V1.1.pdf" H 4500 4650 50  0001 C CNN
F 4 "IN-S63AT5UW" H 4500 4650 50  0001 C CNN "MPN"
	1    4500 4650
	0    -1   -1   0   
$EndComp
$Comp
L Device:R R2
U 1 1 61918782
P 2750 5350
F 0 "R2" H 2820 5396 50  0000 L CNN
F 1 "1k" H 2820 5305 50  0000 L CNN
F 2 "Resistor_SMD:R_0603_1608Metric_Pad0.98x0.95mm_HandSolder" V 2680 5350 50  0001 C CNN
F 3 "https://www.seielect.com/catalog/sei-rmcf_rmcp.pdf" H 2750 5350 50  0001 C CNN
F 4 "RMCF0603JJ1K00" H 2750 5350 50  0001 C CNN "MPN"
	1    2750 5350
	1    0    0    -1  
$EndComp
$Comp
L power:GND #PWR0109
U 1 1 619192D1
P 2750 5500
F 0 "#PWR0109" H 2750 5250 50  0001 C CNN
F 1 "GND" H 2755 5327 50  0000 C CNN
F 2 "" H 2750 5500 50  0001 C CNN
F 3 "" H 2750 5500 50  0001 C CNN
	1    2750 5500
	1    0    0    -1  
$EndComp
Wire Wire Line
	1000 4800 1000 5000
Wire Wire Line
	4500 5000 4500 4800
Wire Wire Line
	4000 5000 4000 4800
Wire Wire Line
	1000 5000 1500 5000
Connection ~ 4000 5000
Wire Wire Line
	4000 5000 4500 5000
Wire Wire Line
	3500 4800 3500 5000
Connection ~ 3500 5000
Wire Wire Line
	3500 5000 4000 5000
Wire Wire Line
	3000 4800 3000 5000
Connection ~ 3000 5000
Wire Wire Line
	3000 5000 3500 5000
Wire Wire Line
	2500 4800 2500 5000
Connection ~ 2500 5000
Wire Wire Line
	2500 5000 2750 5000
Wire Wire Line
	2000 4800 2000 5000
Connection ~ 2000 5000
Wire Wire Line
	2000 5000 2500 5000
Wire Wire Line
	1500 4800 1500 5000
Connection ~ 1500 5000
Wire Wire Line
	1500 5000 2000 5000
Text Label 5500 3500 0    50   ~ 0
LED8
Wire Wire Line
	2750 5200 2750 5000
Connection ~ 2750 5000
Wire Wire Line
	2750 5000 3000 5000
Text Label 1000 4500 1    50   ~ 0
LED1
Text Label 1500 4500 1    50   ~ 0
LED2
Text Label 2000 4500 1    50   ~ 0
LED3
Text Label 2500 4500 1    50   ~ 0
LED4
Text Label 3000 4500 1    50   ~ 0
LED5
Text Label 3500 4500 1    50   ~ 0
LED6
Text Label 4000 4500 1    50   ~ 0
LED7
Text Label 4500 4500 1    50   ~ 0
LED8
$Comp
L mClara:BK-890 BT1
U 1 1 618E2B29
P 1000 1500
F 0 "BT1" V 954 1630 50  0000 L CNN
F 1 "BK-890" V 1045 1630 50  0000 L CNN
F 2 "BAT_BK-890" H 1000 1500 50  0001 L BNN
F 3 "" H 1000 1500 50  0001 L BNN
F 4 "Manufacturer Recommendations" H 1000 1500 50  0001 L BNN "STANDARD"
F 5 "3.00mm" H 1000 1500 50  0001 L BNN "MAXIMUM_PACKAGE_HEIGHT"
F 6 "MPD" H 1000 1500 50  0001 L BNN "MANUFACTURER"
F 7 "E" H 1000 1500 50  0001 L BNN "PARTREV"
	1    1000 1500
	0    1    1    0   
$EndComp
Wire Wire Line
	1000 1200 1000 900 
Wire Wire Line
	1000 1800 1000 2000
Connection ~ 1000 2000
$Comp
L power:+3V0 #PWR0110
U 1 1 618EAC44
P 2500 1000
F 0 "#PWR0110" H 2500 850 50  0001 C CNN
F 1 "+3V0" H 2515 1173 50  0000 C CNN
F 2 "" H 2500 1000 50  0001 C CNN
F 3 "" H 2500 1000 50  0001 C CNN
	1    2500 1000
	1    0    0    -1  
$EndComp
$Comp
L power:+BATT #PWR0111
U 1 1 618EC2FC
P 1000 900
F 0 "#PWR0111" H 1000 750 50  0001 C CNN
F 1 "+BATT" H 1015 1073 50  0000 C CNN
F 2 "" H 1000 900 50  0001 C CNN
F 3 "" H 1000 900 50  0001 C CNN
	1    1000 900 
	1    0    0    -1  
$EndComp
Connection ~ 1000 900 
Wire Wire Line
	1000 900  1500 900 
Connection ~ 2500 1000
Wire Wire Line
	2500 1000 3350 1000
Wire Wire Line
	1900 1000 2250 1000
Text Label 4000 1000 0    50   ~ 0
VL
Text Label 4500 5000 0    50   ~ 0
V1
$EndSCHEMATC
