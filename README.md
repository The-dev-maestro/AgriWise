# 🌱 AgriWise — Carnet de Culture Intelligent & Sécurité Sanitaire

**Projet Mobile .NET MAUI — Activité 7 & Examen Officiel**  
**Porteur du projet :** Kaje Boukem Warren • **Domaine :** Agriculture Durable & Santé Publique • **Région :** Ouest Cameroun (Foumbot / Bafoussam)

---

## 📱 Présentation du Projet
**AgriWise** est une application mobile conçue sur mesure pour les petits producteurs et maraîchers d'Afrique Centrale (Cameroun). Elle répond à une problématique sanitaire critique : **l'absence de traçabilité des intrants chimiques et le non-respect des Délais Avant Récolte (DAR)** qui expose 74% des producteurs et des familles consommatrices à des intoxications chroniques.

AgriWise agit comme un conseiller agricole numérique de poche qui fonctionne **100% hors-ligne**, calcule les dosages exacts par pulvérisateur à dos de 15L, alerte sur les périodes de toxicité et valorise financièrement les récoltes saines certifiées sans résidus.

---

## 🎯 Interfaces Intégrées (Frontend Complet)

1. **Écran d'Accueil / Onboarding (`OnboardingPage.xaml`)** :
   - Présentation de la proposition de valeur AgriWise : *Cultivez sain, dépensez moins, récoltez sans danger*.
   - Badges de fonctionnalités clés : Calculateur DAR, Dosage Pulvérisateur, Santé des sols et familles.
   - Mode 100% Hors-Ligne certifié pour les zones rurales.

2. **Connexion & Profil Agriculteur (`LoginPage.xaml`)** :
   - Saisie rapide par numéro de téléphone camerounais (+237 MTN / Orange).
   - Sélection du bassin agricole (Foumbot, Moungo, Obala, Buea).
   - Choix interactif des spéculations (Tomate Cobra F1, Maïs, Cacao, Piment).
   - Code PIN à 4 chiffres adapté aux conditions de travail au champ.

3. **Tableau de Bord des Parcelles (`DashboardPage.xaml`)** :
   - Bandeau météo localisé (Foumbot • 26°C) et indicateur de statut hors-ligne.
   - **Bannière d'alerte critique DAR** : Compte à rebours J-11 avant récolte avec jauge de dégradation chimique.
   - Cartes d'état des parcelles (Cacao certifié sain, Tomate maraîchère sous DAR, Maïs hybride en floraison).
   - Raccourcis d'assistance et contact direct du conseiller agricole du centre polyvalent.

4. **Journal de Culture Chronologique (`JournalPage.xaml`)** :
   - Fiche technique de parcelle (0,8 ha, humidité sol 68%, variété Cobra F1).
   - Historique horodaté des opérations culturales (Traitements phytosanitaires, fertilisation NPK, irrigation).
   - Indicateurs de respect des doses prescrites et notes vocales des agriculteurs.
   - Totalisation automatique des charges engagées.

5. **Guide Sanitaire & Fiches Intrants Homologués (`HealthGuidePage.xaml`)** :
   - Alertes sanitaires MINADER et sensibilisation aux bonnes pratiques phytosanitaires.
   - Moteur de recherche et fiches techniques d'intrants (Mancozèbe 80 WP, Bouillie Bordelaise, Lambda-cyhalothrine, Purin bio).
   - Protocoles d'urgence et numéros d'assistance anti-poison (SAMU 119).

6. **Calculateur Pulvérisateur & Simulation DAR (`CalculatorPage.xaml`)** :
   - Sélecteur de volume de cuve (10L, 15L standard, 16L, 20L).
   - Calcul précis de la dose nette au gramme près (ex: 150g = 3 cuillères rases) pour éviter le surdosage foliaire.
   - Évaluation de l'économie financière réalisée par pulvérisation.
   - Guide pas-à-pas en 4 étapes de préparation de la bouillie et port des EPI.

7. **Bilan Financier & Rentabilité d'Exploitation (`FinancesPage.xaml`)** :
   - Bénéfice net projeté (+285 500 FCFA) et calcul du Retour sur Investissement (ROI +126%).
   - Carte d'économies directes AgriWise (+48 000 FCFA d'intrants épargnés).
   - Ventilation analytique des charges (intrants, main d'oeuvre, semences, carburant motopompe).
   - Cours du marché de Foumbot et prime de qualité sans résidus (+15% grossistes).

---

## 🏗️ Architecture Technique
- **Framework :** .NET 8.0 MAUI (Multi-platform App UI)
- **Cible Principale :** Android (API 21+) & MacCatalyst
- **Design System :** Palette Nature & Émeraude Africaine (`#103723`, `#1E4D36`, `#86EF7D`, `#27AE60`, `#E67E22`)
- **Iconographie :** 100% Vectorielle (SVG haute fidélité intégrée, sans aucun emoji textuel)
- **Navigation :** Shell déclaratif avec TabBar moderne 4 sections et navigation modale hiérarchique

---

## 📦 Historique des Commits (Phases d'Évaluation)
- **Phase A :** Initialisation du projet .NET MAUI & Design System (Architecture, Thèmes, Typographie)
- **Phase B :** Interfaces Onboarding et Authentification Agriculteur (OnboardingPage & LoginPage)
- **Phase C :** Tableau de Bord Parcelle, Journal de Culture & Guide Sanitaire / Intrants
- **Phase D :** Calculateur Pulvérisateur 15L & Bilan Financier d'Exploitation
- **Phase E :** Raffinement UI, Iconographie vectorielle professionnelle & Finalisation Frontend
