import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
from sklearn.decomposition import PCA
from sklearn.preprocessing import StandardScaler
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report, confusion_matrix, roc_auc_score

def analyze_agents(file_path):
    df = pd.read_csv(file_path)
    
    df = df[df['State'].isin(['Evacuated', 'DeadVehicle'])].copy()
    
    df['IsEvacuated'] = df['State'].apply(lambda x: 1 if x == 'Evacuated' else 0)
    
    features_corr = ['IsEvacuated', 'InitialFuel', 'InitialDistance', 
                     'TotalPassedDistance', 'TotalNodesPassed', 
                     'PathCalculationsCount', 'TicksInCongestion', 'CongestionRate']
    
    available_features = [f for f in features_corr if f in df.columns]
    
    corr_matrix = df[available_features].corr()
    
    plt.figure(figsize=(10, 8))
    sns.heatmap(corr_matrix, annot=True, cmap='coolwarm', fmt=".2f", vmin=-1, vmax=1)
    plt.title('Кореляцiйна матриця ознак агентiв')
    plt.tight_layout()
    plt.savefig('..\\artifacts\\visualizations\\correlation_heatmap.png', dpi=300)
    print("Saved plot: correlation_heatmap.png")
    
    features_model = [f for f in available_features if f != 'IsEvacuated']
    
    X = df[features_model]
    y = df['IsEvacuated']
    
    scaler = StandardScaler()
    X_scaled = scaler.fit_transform(X)
    
    pca = PCA(n_components=2)
    components = pca.fit_transform(X_scaled)
    
    df_pca = pd.DataFrame(data=components, columns=['PC1', 'PC2'])
    df_pca['State'] = df['State'].values
    
    plt.figure(figsize=(10, 6))
    sns.scatterplot(x='PC1', y='PC2', hue='State', 
                    palette={'Evacuated': '#2ecc71', 'DeadVehicle': '#e74c3c'}, 
                    data=df_pca, alpha=0.7, edgecolor=None)
    
    plt.title('PCA:Розподiл агентiв за двома головними компонентами')
    plt.xlabel(f'Головна компонента 1 ({pca.explained_variance_ratio_[0]*100:.1f}% iнформацiї)')
    plt.ylabel(f'Головна компонента 2 ({pca.explained_variance_ratio_[1]*100:.1f}% iнформацiї)')
    plt.tight_layout()
    plt.savefig('..\\artifacts\\visualizations\\pca_scatter.png', dpi=300)
    print("Saved plot: pca_scatter.png")

    components_df = pd.DataFrame(pca.components_.T, columns=['PC1', 'PC2'], index=features_model)
    plt.figure(figsize=(8, 6))
    sns.heatmap(components_df, annot=True, cmap='coolwarm', center=0)
    plt.title('PCA власні вектори: Вклад ознак у головні компоненти')
    plt.tight_layout()
    plt.savefig('..\\artifacts\\visualizations\\pca_eigenvectors.png', dpi=300)
    print("Saved plot: pca_eigenvectors.png")
    print("\n--- Eigenvectors of PCA (That contribute to PC1 and PC2) ---")
    print(components_df)

    X_train, X_test, y_train, y_test = train_test_split(
        X_scaled, y, test_size=0.2, random_state=42, stratify=y
    )

    rf_model = RandomForestClassifier(n_estimators=100, random_state=42)
    rf_model.fit(X_train, y_train)

    y_pred = rf_model.predict(X_test)
    y_proba = rf_model.predict_proba(X_test)[:, 1]

    print("\n--- Quality Metrics of Random Forest Model (on Test Set) ---")

    report = classification_report(y_test, y_pred, target_names=['DeadVehicle', 'Evacuated'])
    roc_auc = roc_auc_score(y_test, y_proba)

    final_report = f"Звіт класифікації:\n{report}\nROC-AUC Score: {roc_auc:.4f}"

    print(report)
    print(f"ROC-AUC Score: {roc_auc:.4f}\n")

    with open("..\\artifacts\\reports\\rf_training_report.txt", "w", encoding="utf-8") as file:
        file.write(final_report)

    plt.figure(figsize=(6, 5))
    cm = confusion_matrix(y_test, y_pred)
    sns.heatmap(cm, annot=True, fmt='d', cmap='Blues', 
                xticklabels=['DeadVehicle', 'Evacuated'], 
                yticklabels=['DeadVehicle', 'Evacuated'])
    plt.title('Confusion Matrix (Random Forest)')
    plt.ylabel('True Class')
    plt.xlabel('Predicted Class')
    plt.tight_layout()
    plt.savefig('..\\artifacts\\visualizations\\confusion_matrix.png', dpi=300)
    print("Saved plot: confusion_matrix.png")

    importance_df = pd.DataFrame({
        'Feature': features_model,
        'Importance': rf_model.feature_importances_
    }).sort_values(by='Importance', ascending=False)

    plt.figure(figsize=(10, 6))
    sns.barplot(x='Importance', y='Feature', data=importance_df, palette='viridis')
    plt.title('Feature Importance (Random Forest): Які ознаки найбільше впливають на виживання?')
    plt.xlabel('Вiдносна важливість (0.0 - 1.0)')
    plt.tight_layout()
    plt.savefig('..\\artifacts\\visualizations\\feature_importance.png', dpi=300)
    print("Saved plot: feature_importance.png")
    print("\n--- Feature Importance ---")
    print(importance_df.to_string(index=False))


def analyze_edges(file_path):
    df = pd.read_csv(file_path)
    
    bottlenecks = df.sort_values(by=['DeadVehiclesCount', 'CongestionDurationTicks'], ascending=[False, False])
    
    text = "Top-5 Найбільш проблематичних доріг (Bottlenecks):\n"
    text += bottlenecks[['EdgeId', 'Length', 'MaxUtilization', 'CongestionDurationTicks', 'DeadVehiclesCount']].head(5).to_string(index=False)

    with open("..\\artifacts\\reports\\edges_report.txt", "w", encoding="utf-8") as file:
        file.write(text)

    print(text)

def analyze_dynamics(trace_file_path):
    try:
        df = pd.read_csv(trace_file_path)

        if 'Tick' not in df.columns or 'State' not in df.columns:
            print(f"Format {trace_file_path} does not contain columns 'Tick' or 'State'. Dynamics plot skipped.")
            return

        state_counts = df.groupby(['Tick', 'State']).size().unstack(fill_value=0)
        
        plt.figure(figsize=(12, 6))
        
        if 'Evacuated' in state_counts.columns:
            plt.plot(state_counts.index, state_counts['Evacuated'], label='Evacuated (Successful)', color='#2ecc71', linewidth=2)
        if 'DeadVehicle' in state_counts.columns:
            plt.plot(state_counts.index, state_counts['DeadVehicle'], label='Dead Vehicles (Failed)', color='#e74c3c', linewidth=2)
            
        if 'Evacuated' in state_counts.columns and 'DeadVehicle' in state_counts.columns:
            total_resolved = state_counts['Evacuated'] + state_counts['DeadVehicle']+ state_counts['Moving']+ state_counts['PathNotFound']
            survival_rate = (state_counts['Evacuated'] / total_resolved.replace(0, 1)) * 100
            
            ax1 = plt.gca()
            ax2 = ax1.twinx()
            ax2.plot(state_counts.index, survival_rate, label='Рiвень виживання (%)', color='#f39c12', linestyle='--')
            ax2.set_ylabel('Рiвень виживання (%)', color='#f39c12')
            ax2.tick_params(axis='y', labelcolor='#f39c12')
            ax2.set_ylim(-5, 105)

        plt.title('Динаміка евакуації та смертності протягом часу симуляції')
        plt.xlabel('Час (Ticks)')
        ax1.set_ylabel('Кількість агентів')
        ax1.grid(True, linestyle='--', alpha=0.6)
        
        lines1, labels1 = ax1.get_legend_handles_labels()
        lines2, labels2 = ax2.get_legend_handles_labels() if 'ax2' in locals() else ([], [])
        ax1.legend(lines1 + lines2, labels1 + labels2, loc='upper left')
        
        plt.tight_layout()
        plt.savefig('..\\artifacts\\visualizations\\mortality_dynamics.png', dpi=300)
        print("Saved plot: mortality_dynamics.png")
        
    except FileNotFoundError:
        print(f"\nFile {trace_file_path} not found. A log of the streaming telemetry is required for dynamics analysis.")


if __name__ == "__main__":
    print("Starting the analytics pipeline for Urban Evacuation Simulator...\n")
    print("--- Agent Analysis ---")
    analyze_agents('..\\artifacts\\datasets\\agents_dataset.csv')
    
    print("\n--- Edge Analysis ---")
    analyze_edges('..\\artifacts\\datasets\\edges_dataset.csv')
    
    print("\n--- Simulation Dynamics ---")
    analyze_dynamics('..\\artifacts\\datasets\\simulation_trace.csv')