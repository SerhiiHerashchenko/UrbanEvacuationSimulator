import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
from sklearn.decomposition import PCA
from sklearn.preprocessing import StandardScaler

def analyze_agents(file_path):
    df = pd.read_csv(file_path)
    
    df = df[df['State'].isin(['Evacuated', 'DeadVehicle'])].copy()
    
    df['IsEvacuated'] = df['State'].apply(lambda x: 1 if x == 'Evacuated' else 0)
    
    features_corr = ['IsEvacuated', 'InitialFuel', 'InitialDistance', 'Speed', 
                     'TotalPassedDistance', 'TotalNodesPassed', 
                     'PathCalculationsCount', 'TicksInCongestion']
    
    corr_matrix = df[features_corr].corr()
    
    plt.figure(figsize=(10, 8))
    sns.heatmap(corr_matrix, annot=True, cmap='coolwarm', fmt=".2f", vmin=-1, vmax=1)
    plt.title('Matrix of Correlations of Agent Features')
    plt.tight_layout()
    plt.savefig('correlation_heatmap.png', dpi=300)
    print("✅ Saved plot: correlation_heatmap.png")
    
    features_pca = ['InitialFuel', 'InitialDistance', 'TotalPassedDistance', 
                    'TotalNodesPassed', 'PathCalculationsCount', 'TicksInCongestion']
    
    X = df[features_pca]
    
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
    
    plt.title('PCA: Distribution of Agents (2 Principal Components)')
    plt.xlabel(f'Principal Component 1 ({pca.explained_variance_ratio_[0]*100:.1f}% information)')
    plt.ylabel(f'Principal Component 2 ({pca.explained_variance_ratio_[1]*100:.1f}% information)')
    plt.tight_layout()
    plt.savefig('pca_scatter.png', dpi=300)
    print("✅ Saved plot: pca_scatter.png")

def analyze_edges(file_path):
    df = pd.read_csv(file_path)
    
    bottlenecks = df.sort_values(by=['DeadVehiclesCount', 'CongestionDurationTicks'], ascending=[False, False])
    
    print("\n🚨 Top-5 Most Problematic Sections (Bottlenecks):")
    print(bottlenecks[['EdgeId', 'Length', 'MaxUtilization', 'CongestionDurationTicks', 'DeadVehiclesCount']].head(5).to_string(index=False))

if __name__ == "__main__":
    print("Starting the analytics pipeline for Urban Evacuation Simulator...\n")
    print("--- Agent Analysis ---")
    analyze_agents('agents_dataset.csv')
    
    print("\n--- Edge Analysis ---")
    analyze_edges('edges_dataset.csv')