<script setup lang="ts">
import { useData } from "vitepress";
import { computed } from "vue";

interface HeroAction {
  theme: string;
  text: string;
  link: string;
}

interface Hero {
  name: string;
  text: string;
  tagline: string;
  actions: HeroAction[];
}

interface Feature {
  title: string;
  details: string;
}

interface FrontmatterData {
  hero: Hero;
  features: Feature[];
}

const { frontmatter } = useData();

const hero = computed(() => (frontmatter.value?.hero || {}) as Hero);
const features = computed(
  () => (frontmatter.value?.features || []) as Feature[]
);
</script>

<template>
  <div class="VPHome">
    <div class="home-hero-with-video">
      <div class="home-hero-content">
        <div class="VPHero">
          <div class="container">
            <div class="main">
              <h1 class="name">
                <span class="clip">{{ hero.name }}</span>
              </h1>
              <p class="text">{{ hero.text }}</p>
              <p class="tagline">{{ hero.tagline }}</p>
              <div class="actions">
                <div
                  v-for="action in hero.actions"
                  :key="action.link"
                  class="action"
                >
                  <a
                    :class="action.theme"
                    class="VPButton"
                    :href="action.link"
                    >{{ action.text }}</a
                  >
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="home-hero-video">
        <iframe
          width="560"
          height="315"
          src="https://www.youtube.com/embed/JlY0KBnzCzA?si=_QwGuzB3SFmkLSpW"
          title="YouTube video player"
          frameborder="0"
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
          referrerpolicy="strict-origin-when-cross-origin"
          allowfullscreen
        ></iframe>
      </div>
    </div>
    <div class="VPFeatures">
      <div class="container">
        <div class="items">
          <div v-for="feature in features" :key="feature.title" class="item">
            <div class="VPFeature">
              <article class="box">
                <h2 class="title">{{ feature.title }}</h2>
                <p class="details">{{ feature.details }}</p>
              </article>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.VPHome {
  padding: 64px 24px;
}

.home-hero-with-video {
  display: flex;
  align-items: center;
  gap: 48px;
  max-width: 1152px;
  margin: 0 auto 64px;
}

.home-hero-content {
  flex: 1;
  min-width: 300px;
}

.clip {
  background: rgb(168, 177, 255);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.name {
  font-size: 48px;
  font-weight: 700;
  line-height: 1.2;
  margin-bottom: 8px;
}

.text {
  font-size: 36px;
  font-weight: 600;
  line-height: 1.2;
  margin-bottom: 16px;
  color: var(--vp-c-text-1);
}

.tagline {
  font-size: 20px;
  line-height: 1.4;
  color: var(--vp-c-text-2);
  margin-bottom: 32px;
}

.actions {
  display: flex;
  gap: 12px;
}

.action .VPButton {
  padding: 12px 24px;
  font-size: 16px;
  font-weight: 600;
  border-radius: 20px;
  transition: all 0.2s;
}

.action .VPButton:hover {
  transform: translateY(-1px);
}

.home-hero-video {
  flex: 1;
  max-width: 560px;
}

.home-hero-video iframe {
  width: 100%;
  aspect-ratio: 16/9;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
  border: none;
}

.VPFeatures {
  max-width: 1152px;
  margin: 0 auto;
}

.items {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 24px;
}

.VPFeature {
  height: 100%;
}

.box {
  height: 100%;
  padding: 24px;
  border-radius: 12px;
  background: var(--vp-c-bg-soft);
  transition: all 0.2s;
}

.title {
  font-size: 20px;
  font-weight: 600;
  line-height: 1.4;
  margin-bottom: 8px;
  color: var(--vp-c-text-1);
}

.details {
  font-size: 16px;
  line-height: 1.6;
  color: var(--vp-c-text-2);
}

@media (max-width: 960px) {
  .home-hero-with-video {
    flex-direction: column;
    text-align: center;
    gap: 32px;
  }

  .actions {
    justify-content: center;
  }

  .name {
    font-size: 40px;
  }

  .text {
    font-size: 32px;
  }
}

@media (max-width: 640px) {
  .VPHome {
    padding: 32px 16px;
  }

  .name {
    font-size: 32px;
  }

  .text {
    font-size: 24px;
  }

  .tagline {
    font-size: 18px;
  }
}
</style>
