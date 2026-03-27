# Changelog
Všechny významné změny ve Vehimapu budou zapisovány sem.
Formát vychází z [Keep a Changelog](https://keepachangelog.com/cs/1.1.0/)
a projekt používá [Semantic Versioning](https://semver.org/lang/cs/).

## [Unreleased]

### Přidáno
- nový přehled `Audit dat`, který sjednocuje chybějící povinné údaje, neplatné rozsahy, problematické doklady, podezřelé tachometry, nákladové nesrovnalosti i servisní plány bez použitelného odometru
- explicitní servisní profil vozidla s poli `Pohon`, `Klimatizace`, `Rozvody` a `Převodovka`, který slouží jako základ pro doporučené servisní šablony
- výběrový dialog doporučených servisních šablon, ve kterém lze návrhy před přidáním odškrtnout nebo upravit
- automatická nabídka doporučených servisních šablon hned po založení nového vozidla
- nový `Balíček pro vozidlo`, který umí v jednom kroku nabídnout doporučené servisní plány, placeholdery dokladů a obecné připomínky podle kategorie i servisního profilu vozidla
- akce v dashboardu pro rychlé otevření historie vozidla a okamžité označení servisního úkonu jako splněného
- samostatný přehled `Náklady napříč vozidly` pro porovnání vozidel v jednom období

### Změněno
- pole `Typ` u vozidla bylo nahrazeno praktičtější `Poznámkou k vozidlu`
- přehledy termínů a dashboard lépe zvýrazňují problémové stavy, datové nedostatky a servisní úkoly
- Vehimap je interně rozdělený do menších modulů `#Include`, takže se aplikace lépe udržuje a rozvíjí

### Opraveno
- zpracování servisních doporučení, záloh a meta dat vozidel tak, aby správně fungoval nový servisní profil i smoke testy

## [1.0.2] - 2026-03-27

### Opraveno
- kontrola aktualizací ve zkompilované portable aplikaci teď správně načítá veřejný release manifest
- doplněny smoke testy pro kontrolu aktualizací a načítání update manifestu

## [1.0.1] - 2026-03-27

### Přidáno
- první veřejné vydání aplikace Vehimap
- evidence vozidel, historie událostí, kilometrů a tankování, pojištění a dokladů i vlastních připomínek
- dashboard, přehled blížících se a propadlých termínů, globální hledání a rychlé filtrování v jednotlivých evidencích
- exporty, import dat, automatické zálohy, klávesové zkratky a další úpravy přístupnosti
- ruční kontrola aktualizací a portable aktualizace aplikace z GitHub release
