# Geolocation OSINT

The discipline of pinpointing where a photo, video, or claim was made — using only public information.

> **Bellingcat methodology** is the canonical reference. Ten Bellingcat geolocation case studies will teach more than any cheatsheet — but here's the structured toolkit.

---

## Workflow

```
1. Triage: what's the claim? what's plausible?
2. Image triage: extract every visible feature (signs, logos, vegetation, road markings, vehicles, architecture, sun angle)
3. Wide search: language detection, OCR, reverse image search → narrow to country/region
4. Narrow search: cross-reference features against satellite / street view
5. Verify: at least three independent matches before "geolocated"
6. Time verification: shadow / weather / traffic / timestamp validation
7. Document: capture URLs + lat/lon + reasoning chain
```

---

## Reverse Image Search (start here)

| Engine | Best at |
|:-------|:--------|
| **Yandex Images** ([yandex.com/images](https://yandex.com/images/)) | **Faces** + Cyrillic-language sites + Eastern European content. Often the single most useful for geolocation |
| **Google Images** | Largest index, broad |
| **TinEye** ([tineye.com](https://tineye.com/)) | Best for finding the **earliest** posted version of an image |
| **Bing Visual Search** | Sometimes catches what Google missed |
| **Baidu Images** | Best for Chinese-hosted content — see [[Regional_China]] |
| **PimEyes** (paid) | Face-only, very effective — see [[Commercial_Tools]] |

**RevEye** / **Search by Image** browser extensions ([[Browser_Extensions#Reverse Image Search & Visual]]) right-click the same image into all engines simultaneously.

---

## Map / Satellite / Street View

### Multi-engine map switchers
- **Map Switcher** browser extension — single click, opens same coords in Google / Yandex / Bing / OSM / Apple
- **Geohack** ([geohack.toolforge.org](https://geohack.toolforge.org/)) — Wikipedia's gateway to dozens of map services from one coord

### Satellite imagery
| Service | Notes |
|:--------|:------|
| **Google Earth Pro** (free desktop) | **Historical imagery layer** is huge — useful for dating photos / tracking changes |
| **Sentinel Hub / Copernicus EO Browser** ([sentinel-hub.com/explore/eobrowser](https://www.sentinel-hub.com/explore/eobrowser/)) | Free European Space Agency satellite imagery, recent |
| **NASA Worldview** | Daily satellite imagery, weather, fire detection |
| **Planet Labs** | Commercial — daily 3m imagery, paid |
| **Maxar / Capella Space / ICEYE** | Commercial high-res, sometimes Twitter-posted snapshots of newsworthy events |
| **USGS EarthExplorer** | Free, includes historical Landsat back to 1972 |

### Street view (provider-specific by region)
| Region | Best provider |
|:-------|:--------------|
| **Most of world** | Google Street View |
| **Russia, CIS** | Yandex Panoramas — covers areas Google doesn't, see [[Regional_RUNet]] |
| **South Korea** | **Naver Maps "거리뷰" + Kakao "로드뷰"** — Google Maps is restricted in KR, see [[Country_SouthKorea]] |
| **Japan** | Google Street View, plus Mapion / NAVITIME |
| **China** | Tencent Maps + AMap (Baidu Maps weakest for street view) — see [[Regional_China]] |
| **Where Google lacks coverage** | **Mapillary** ([mapillary.com](https://www.mapillary.com/)) — crowdsourced street imagery, often covers rural areas |
| **Ditto** | **KartaView** (formerly OpenStreetView) |

---

## Specific Feature → Lookup

### Sun position / shadows
- **SunCalc** ([suncalc.org](https://www.suncalc.org/)) — sun position for any lat/lon and time, draws shadow direction
- **SunCalc.net** — sister tool, same data
- **Heavens-Above** — also handles sun + moon

### Weather verification
- **Time and Date** ([timeanddate.com/weather](https://www.timeanddate.com/weather/)) — historical weather by city
- **Weather Underground (Wunderground)** — historical hourly observations
- **OGIMET** — METAR archive (airports, very reliable for past weather)
- **Worldview MODIS Aqua / Terra** — cloud cover from satellite

### Aircraft / flights
- **FlightRadar24** ([flightradar24.com](https://www.flightradar24.com/)) — civilian flights, real-time + historical (paid for full history)
- **FlightAware** — flight data, US-strong
- **ADS-B Exchange** ([adsbexchange.com](https://globe.adsbexchange.com/)) — **uncensored** ADS-B, including military traffic FlightRadar filters
- **OpenSky Network** ([opensky-network.org](https://opensky-network.org/)) — academic ADS-B data, free historical
- **Plane Finder** — alternative
- **AirNav RadarBox** — alternative

### Ships / maritime
- **MarineTraffic** ([marinetraffic.com](https://www.marinetraffic.com/)) — AIS data, paid for history
- **VesselFinder** — alternative
- **MyShipTracking** — free
- **Lloyd's List Intelligence** — paid, gold-standard for sanctions / ownership tracking

### Vehicles / license plates
- Country-specific plate format references — [worldlicenseplates.com](http://www.worldlicenseplates.com/)
- **NumberPlatesRegister.com** — UK
- **Carjam** (NZ), **Carfax** (US/CA) — vehicle history

### Architectural / vegetation
- **Mapillary** — crowd-sourced street imagery often catches non-Google-covered areas
- **Floraweb / iNaturalist** — vegetation identification (helps narrow regions)
- **Bellingcat's Online Investigation Toolkit** — links to architecture-by-region resources

### Cell towers
- **OpenCellID** ([opencellid.org](https://www.opencellid.org/)) — cell tower locations worldwide, free
- **CellMapper** — community crowd-sourced
- **Mozilla Location Service** — discontinued but archives exist

### Telephone country codes
- **Wikipedia: List of country calling codes** — fast reference
- **Numverify, Truecaller** — number → country/carrier lookup

### Time zones
- **Time.is** — current time anywhere
- **Wolfram Alpha** — "what time was it in [city] on [date] at [UTC time]"

---

## Specific Geolocation Workflows

### Photo with people / places
1. Reverse image search (Yandex first, then Google + TinEye)
2. Identify language of any signs (Google Translate camera mode, OCR)
3. Extract place names from signs → search on Google Maps + the regional map provider
4. Match architectural features (window styles, roof pitches, balconies, rooflines) to candidate region
5. Once narrowed: walk the area in street view comparing features

### Video with movement
1. Frame-by-frame extract distinctive features (signs, landmarks, vehicles)
2. Watch traffic flow direction — left vs right driving narrows country
3. Listen for language / accent on audio
4. Listen for sirens / vehicle types (European emergency siren patterns differ from US)
5. Match camera path to OSM / street view

### Aerial / drone footage
1. Match terrain features to satellite imagery
2. Use sun angle + shadows to estimate time of day → narrow date range
3. Check Sentinel Hub for recent passes that might match cloud / vegetation state

### Indoor photo
- Hardest case. Look for:
  - Power outlets (different per region)
  - Plug types (UK vs Europlug vs US vs China)
  - TV broadcast logos / channels
  - Mobile network display
  - Light switches (orientation, style by region)
  - Magazines / books / TV in background

### Historical events / dating photos
- **InVID-WeVerify** (browser plugin) — video forensics + reverse search
- **FotoForensics** ([fotoforensics.com](https://fotoforensics.com/)) — error-level analysis (manipulation detection)
- **EXIF metadata** via [[Tools_Kali_Tracelabs#ExifTool|ExifTool]] — DateTimeOriginal field, GPS if not stripped
- **Google Earth historical imagery** — match imagery date to candidate date

---

## Tooling — Geolocation-Specific

| Tool | Use |
|:-----|:----|
| **GeoSpy** ([geospy.ai](https://geospy.ai/)) | AI-driven photo geolocation (commercial) |
| **Picarta** | AI photo geolocation |
| **Geo-OSINT** Discord communities | Volunteer geolocators willing to crowd-source hard cases |
| **GeoConfirmed** ([geoconfirmed.org](https://geoconfirmed.org/)) | Community-verified geolocations of conflict footage |
| **GeoGuessr-Pros community Discord** | Geolocation experts, often help with OSINT cases pro bono |
| **Tracelabs CTF infrastructure** | Volunteer-driven geolocation as part of missing-persons cases |
| **InVID-WeVerify** | Video frame extraction + reverse image |
| **Bellingcat's online investigation toolkit** ([bellingcat.com/resources](https://www.bellingcat.com/resources/)) | Links to dozens of niche tools |

---

## Verification — When Have You Geolocated?

A geolocation is only complete when you have:

1. **Lat/lon coordinates** (specific to building / street level usually)
2. **At least three independent feature matches** to that exact location
3. **Plausibility check** — does the time / weather / sun angle agree?
4. **Documented chain** — every URL + reasoning step captured ([[Browser_Extensions#Capture & Note-Taking|Hunchly]])

Three matches matter because **single matches lie**. A McDonald's sign matches anywhere; combined with a specific street name + a distinctive building behind it = real geolocation.

> **Cardinal rule:** if the geolocation requires you to *want* to find it there, you haven't found it.

---

## Notable Geolocation Case Studies

Read these to see methodology in action:

- **MH17 (Bellingcat, 2014-2018):** Russian buk launcher tracked by license-plate + dashcam frames + roadside features
- **Skripal poisoning (Bellingcat, 2018):** Salisbury CCTV + flight manifests + geographically-anchored social-media posts
- **Mahsa Amini protests (2022):** GeoConfirmed + IranWire verified hundreds of incidents
- **Mariupol drama theatre bombing (2022):** Bellingcat geolocated based on building façade and adjacent landmarks
- **Hamas Oct 7 2023 / IDF Gaza operations:** GeoConfirmed and others verified hundreds of videos
- **Capitol Riot Jan 6 2021:** Sedition Hunters identified locations within Capitol via wall mural + door details

---

## Common Pitfalls

| Pitfall | Why it bites |
|:--------|:------------|
| **Trusting a single visual match** | Visual coincidences abound at low resolution |
| **Ignoring sun direction** | Shadows are ground truth; can rule out "your" candidate location |
| **Mistaking right-hand for left-hand drive** | Easy to flip mentally, narrows country instantly |
| **Falling for AI-generated / staged images** | Run FotoForensics + check for impossible details (six-finger hands, garbled text) |
| **Accepting a Twitter caption as ground truth** | Captions are wrong constantly — verify independently |
| **Forgetting time zones** | Sun angle requires UTC + lat/lon, not local-time guess |
| **Ignoring that the photo could be old** | Yandex / TinEye reverse-search to find earliest posting |

---

## See Also

- [[OSINT]] — Folder index
- [[Browser_Extensions]] — RevEye, Map Switcher, InVID-WeVerify, Wayback
- [[Tools_Kali_Tracelabs]] — ExifTool, Photon, Metagoofil
- [[VMs_and_Compartmentalization]] — Always investigate from a clean VM
- [[Commercial_Tools]] — GeoSpy, Picarta, ShadowDragon, PimEyes
- [[OSS_Tools]] — InVID-WeVerify, FotoForensics, ExifTool, Sentinel Hub
- [[Country_USA]] — Capitol-riot crowd-source case
- [[Country_Belarus]] — Ryanair flight FR4978 hijack case
- [[Country_Iran]] — Mahsa Amini protest verification
- [[Country_NorthKorea]] — Satellite imagery as primary methodology
