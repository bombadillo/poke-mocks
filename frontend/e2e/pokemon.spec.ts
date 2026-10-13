import { expect, test } from '@playwright/test'
import { loadFixture, stubPokemonApi, PokemonFixture } from './fixtures'

test('renders names and the total count from the mapped API response', async ({
    page
}) => {
    const fixture = loadFixture('initial-load') as PokemonFixture
    await stubPokemonApi(page, fixture)

    await page.goto('/')

    await expect(
        page.getByRole('heading', { name: 'First discoveries' })
    ).toBeVisible()
    await expect(page.getByRole('listitem')).toHaveCount(3)
    await expect(page.getByText('bulbasaur')).toBeVisible()
    await expect(page.getByText('ivysaur')).toBeVisible()
    await expect(page.getByText('venusaur')).toBeVisible()
    await expect(page.getByText('3 shown / 1302 total')).toBeVisible()

    await expect(page.getByTestId('pokemon-list')).toHaveScreenshot(
        'pokemon-list.png'
    )
})

test('shows a loading state while the API request is pending', async ({
    page
}) => {
    const fixture = loadFixture('initial-load') as PokemonFixture
    let release!: () => void
    const gate = new Promise<void>((resolve) => (release = resolve))

    await page.route('**/api/pokemon', async (route) => {
        await gate // hold the response until we've taken the screenshot
        await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify(fixture)
        })
    })

    await page.goto('/')

    const status = page.getByRole('status')
    await expect(status).toHaveText('Loading Pokémon...')
    await expect(status).toHaveScreenshot('loading-state.png')

    release()
    await expect(page.getByText('bulbasaur')).toBeVisible()
})

test('shows an empty state for an empty collection', async ({ page }) => {
    await stubPokemonApi(page, loadFixture('empty-results'))

    await page.goto('/')

    const status = page.getByRole('status')
    await expect(status).toHaveText(
        'No Pokémon found. Check back after your next adventure.'
    )
    await expect(page.getByRole('listitem')).toHaveCount(0)

    await expect(status).toHaveScreenshot('empty-state.png')
})

test('shows a useful error state when the API fails', async ({ page }) => {
    await stubPokemonApi(page, loadFixture('api-error'), 502)

    await page.goto('/')

    const alert = page.getByRole('alert')
    await expect(alert).toHaveText(
        "We couldn't reach the Pokédex right now. Please try again in a moment."
    )

    await expect(alert).toHaveScreenshot('error-state.png')
})
