// Encodes only native Unity QA captures. It never edits the game's source paintings.
// SHARP_PATH can point to another locally installed sharp package.
const fs = require('node:fs');
const path = require('node:path');
const sharp = require(process.env.SHARP_PATH || 'C:/Users/zxc28/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');

async function main() {
  const review = path.resolve(__dirname, '../Review');
  const files = fs.readdirSync(path.join(review, 'animation')).filter(x => /^frame-\d+\.png$/.test(x)).sort();
  const frames = [];
  let width, height;
  for (const file of files) {
    const { data, info } = await sharp(path.join(review, 'animation', file)).removeAlpha().raw().toBuffer({ resolveWithObject: true });
    if (width && (width !== info.width || height !== info.height)) throw new Error('Frame dimensions do not match');
    width = info.width; height = info.height; frames.push(data);
  }
  if (!frames.length) throw new Error('Run Unity verification to produce frames first');
  const target = path.join(review, 'motion-preview.gif');
  await sharp(Buffer.concat(frames), { raw: { width, height: height * frames.length, channels: 3, pageHeight: height } })
    .gif({ colours: 256, effort: 7, dither: 0.6, loop: 0, delay: frames.map(() => 67) }).toFile(target);
  const metadata = await sharp(target, { animated: true }).metadata();
  if (metadata.pages !== frames.length) throw new Error('Animated export lost frames');
  process.stdout.write(JSON.stringify({ output: target, frames: metadata.pages, width: metadata.width, height: metadata.pageHeight }) + '\n');
}
main().catch(error => { process.stderr.write(error.stack + '\n'); process.exitCode = 1; });
